using ProjektGrupowy.Models;
using System.Collections.Concurrent;

namespace ProjektGrupowy.Services
{
    public class LoggingService
    {
        private readonly ConnectionHelper _connectionHelper = new();
        private CancellationTokenSource? _cts;
        private Task? _workerTask;
        private int? _currentTrackId;
        private readonly ConcurrentQueue<GeoLog> _queue = new();
        private readonly int _maxQueue = 40; // allow some backlog

        public bool IsRunning => _workerTask != null && !_workerTask.IsCompleted;

        public async Task StartAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) throw new ArgumentException("nick");
            if (IsRunning) return;

            // create track in DB (best effort)
            _currentTrackId = await _connectionHelper.CreateTrackAsync(nick);

            _cts = new CancellationTokenSource();
            _workerTask = Task.Run(() => WorkerLoop(_cts.Token));
        }

        public async Task StopAsync()
        {
            if (!IsRunning) return;

            _cts?.Cancel();
            try
            {
                await _workerTask!;
            }
            catch (OperationCanceledException) { }
            finally
            {
                // finalize track
                if (_currentTrackId.HasValue)
                {
                    await _connectionHelper.StopTrackAsync(_currentTrackId.Value);
                    _currentTrackId = null;
                }

                // clear queue (we allow losing some points)
                while (_queue.TryDequeue(out _)) { }
            }
        }

        private async Task WorkerLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    // get current location
                    var request = new Microsoft.Maui.Devices.Sensors.GeolocationRequest(Microsoft.Maui.Devices.Sensors.GeolocationAccuracy.Best, TimeSpan.FromSeconds(5));
                    var loc = await Microsoft.Maui.Devices.Sensors.Geolocation.Default.GetLocationAsync(request, token);

                    if (loc != null && _currentTrackId.HasValue)
                    {
                        var geo = new GeoLog
                        {
                            track_id = _currentTrackId.Value,
                            geo_latitude = loc.Latitude,
                            geo_longitude = loc.Longitude,
                            locationDate = DateTime.UtcNow
                        };

                        try
                        {
                            await _connectionHelper.InsertGeoLogAsync(geo.track_id, geo.geo_latitude, geo.geo_longitude, geo.locationDate);

                            // on success, flush queued items
                            await FlushQueueAsync();
                        }
                        catch (Exception)
                        {
                            // enqueue and continue; allow dropping oldest when queue too big
                            _queue.Enqueue(geo);
                            while (_queue.Count > _maxQueue && _queue.TryDequeue(out _)) { }
                        }
                    }
                }
                catch (Exception ex) when (!(ex is OperationCanceledException))
                {
                    // swallow exceptions to avoid crashing app; log to console
                    Console.WriteLine($"Logging worker error: {ex.Message}");
                }

                // wait ~15s between attempts
                try
                {
                    await Task.Delay(TimeSpan.FromSeconds(15), token);
                }
                catch (OperationCanceledException) { }
            }
        }

        private async Task FlushQueueAsync()
        {
            // attempt to send queued items; on any failure leave remaining in queue
            var sent = new List<GeoLog>();
            while (_queue.TryPeek(out var item))
            {
                if (!_queue.TryDequeue(out var toSend)) break;
                try
                {
                    await _connectionHelper.InsertGeoLogAsync(toSend.track_id, toSend.geo_latitude, toSend.geo_longitude, toSend.locationDate);
                }
                catch (Exception)
                {
                    // failed to flush, put back and stop
                    _queue.Enqueue(toSend);
                    break;
                }
            }
        }
    }
}
