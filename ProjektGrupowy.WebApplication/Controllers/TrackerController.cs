using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjektGrupowy.WebApplication.Data;
using ProjektGrupowy.WebApplication.Models;

namespace ProjektGrupowy.WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackerController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TrackerController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromQuery] string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return BadRequest("nick required");
            var track = new Track { nick = nick, trackStart = DateTime.UtcNow };
            _db.Tracks.Add(track);
            await _db.SaveChangesAsync();
            return Ok(new { id = track.id });
        }

        [HttpPost("stop/{id}")]
        public async Task<IActionResult> Stop(int id)
        {
            var track = await _db.Tracks.FindAsync(id);
            if (track == null) return NotFound();
            track.trackStop = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok();
        }

        // Keep DTO type for compatibility with potential hot-reload constraints
        public class GeoLogDto { public int trackId { get; set; } public double lat { get; set; } public double lon { get; set; } public DateTimeOffset timestamp { get; set; } }

        // Return list of tracks for a given nick
        [HttpGet("tracks")]
        public async Task<IActionResult> GetTracks([FromQuery] string nick)
        {
            if (string.IsNullOrWhiteSpace(nick)) return BadRequest("nick required");
            var tracks = await _db.Tracks.Where(t => t.nick == nick).OrderByDescending(t => t.trackStart).ToListAsync();
            return Ok(tracks);
        }

        // Return geologs for a given track id
        [HttpGet("tracks/{id}/logs")]
        public async Task<IActionResult> GetLogs(int id)
        {
            var logs = await _db.GeoLogs.Where(g => g.track_id == id).OrderBy(g => g.locationDate).ToListAsync();
            return Ok(logs);
        }
    }
}
