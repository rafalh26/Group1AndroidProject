using ProjektGrupowy.Models;
using System.Collections.Concurrent;
using Microsoft.Maui.ApplicationModel;

namespace ProjektGrupowy.Services
{
    public class LoggingService
    {
        // Shared singleton instance so other pages can stop logging before quitting
        public static LoggingService Instance { get; } = new LoggingService();

        private readonly ConnectionHelper _connectionHelper = new();
        private CancellationTokenSource? _cts;
        private Task? _workerTask;
        private int? _currentTrackId;
        private string? _nick;
        private DateTime _startedAt;
        private DateTime? _lastSuccessfulDbOp;
        private readonly TimeSpan _offlineTimeout = TimeSpan.FromMinutes(20);
        private readonly ConcurrentQueue<GeoLog> _queue = new();
        private readonly int _maxQueue = 40; // allow some backlog

        // Convert UTC to Poland local time (tries common TZ IDs with fallbacks)
        private static DateTime ConvertToPolandTime(DateTime utc)
        {
            if (utc.Kind != DateTimeKind.Utc)
            {
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);
            }

            string[] tzIds = new[] { "Europe/Warsaw", "Central European Standard Time" };
            foreach (var id in tzIds)
            {
                try
                {
                    var tz = TimeZoneInfo.FindSystemTimeZoneById(id);
                    return TimeZoneInfo.ConvertTimeFromUtc(utc, tz);
                }
                catch (TimeZoneNotFoundException) { }
                catch (InvalidTimeZoneException) { }
            }

            // Fallback: use system local time
            return utc.ToLocalTime();
        }

        public bool IsRunning => _workerTask != null && !_workerTask.IsCompleted;

        public async Task StartAsync(string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) throw new ArgumentException("nick");
            if (IsRunning) return;

            // create track in DB (best effort)
            _nick = nick;
            _startedAt = DateTime.UtcNow;
            _lastSuccessfulDbOp = null;
            try
            {
                _currentTrackId = await _connectionHelper.CreateTrackAsync(nick);
                if (_currentTrackId.HasValue)
                    _lastSuccessfulDbOp = DateTime.UtcNow;
            }
            catch
            {
                _currentTrackId = null;
            }

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
                            // store UTC as DateTimeOffset
                            locationDate = DateTimeOffset.UtcNow
                        };

                        try
                        {
                            await _connectionHelper.InsertGeoLogAsync(geo.track_id, geo.geo_latitude, geo.geo_longitude, geo.locationDate);

                            // on success, flush queued items
                            await FlushQueueAsync();
                            _lastSuccessfulDbOp = DateTime.UtcNow;
                        }
                        catch (Exception)
                        {
                            // enqueue and continue; allow dropping oldest when queue too big
                            _queue.Enqueue(geo);
                            while (_queue.Count > _maxQueue && _queue.TryDequeue(out _)) { }
                        }
                    }
                    else if (loc != null)
                    {
                        // No DB track yet: queue point with placeholder track id (-1)
                        var queued = new GeoLog
                        {
                            track_id = -1,
                            geo_latitude = loc.Latitude,
                            geo_longitude = loc.Longitude,
                            // store UTC as DateTimeOffset for queued items as well
                            locationDate = DateTimeOffset.UtcNow
                        };
                        _queue.Enqueue(queued);
                        while (_queue.Count > _maxQueue && _queue.TryDequeue(out _)) { }
                    }

                    // If we don't have a DB track id, try to create one (best-effort) so queued points can be flushed.
                    if (!_currentTrackId.HasValue && !string.IsNullOrWhiteSpace(_nick))
                    {
                        try
                        {
                            var created = await _connectionHelper.CreateTrackAsync(_nick);
                            if (created.HasValue)
                            {
                                _currentTrackId = created.Value;
                                _lastSuccessfulDbOp = DateTime.UtcNow;
                                // attempt to flush queued items now that we have a track id
                                await FlushQueueAsync();
                            }
                        }
                        catch (Exception ex)
                        {
                            // ignore - we'll retry later
                            Console.WriteLine($"Track creation retry failed: {ex.Message}");
                        }
                    }

                    // If we've been offline (no DB success) longer than allowed, quit the app
                    if (!_currentTrackId.HasValue && DateTime.UtcNow - _startedAt > _offlineTimeout && !_lastSuccessfulDbOp.HasValue)
                    {
                        try
                        {
                            MainThread.BeginInvokeOnMainThread(() => Application.Current.Quit());
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to quit app after offline timeout: {ex.Message}");
                        }
                        break;
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
                    // If queued item was created before track existed, remap its track id to the current track id
                    var trackId = toSend.track_id < 0 && _currentTrackId.HasValue ? _currentTrackId.Value : toSend.track_id;
                    if (!_currentTrackId.HasValue && toSend.track_id < 0)
                    {
                        // no track yet; put back and stop flushing
                        _queue.Enqueue(toSend);
                        break;
                    }
                    await _connectionHelper.InsertGeoLogAsync(trackId, toSend.geo_latitude, toSend.geo_longitude, toSend.locationDate);
                    _lastSuccessfulDbOp = DateTime.UtcNow;
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
