using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjektGrupowy.WebApplication.Data;
using ProjektGrupowy.WebApplication.Models;
using System.Security.Cryptography;

namespace ProjektGrupowy.WebApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AccountController(AppDbContext db)
        {
            _db = db;
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(input);
            var hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User model, [FromQuery] string? password)
        {
            if (model == null || string.IsNullOrWhiteSpace(model.nick))
                return BadRequest("Invalid user");

            var existing = await _db.users.FirstOrDefaultAsync(u => u.nick == model.nick);
            var hash = password == null ? model.pass_hash : ComputeHash(password);

            if (existing == null)
            {
                model.pass_hash = hash ?? string.Empty;
                model.created_at = DateTime.UtcNow;
                _db.users.Add(model);
                await _db.SaveChangesAsync();
                return Ok(model);
            }

            // Update existing simple upsert behavior
            existing.name = model.name ?? existing.name;
            existing.email = model.email ?? existing.email;
            existing.phone = model.phone ?? existing.phone;
            existing.address = model.address ?? existing.address;
            if (!string.IsNullOrWhiteSpace(hash)) existing.pass_hash = hash;
            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromQuery] string nick, [FromQuery] string password)
        {
            if (string.IsNullOrWhiteSpace(nick)) return BadRequest("nick required");
            var user = await _db.users.FirstOrDefaultAsync(u => u.nick == nick);
            if (user == null) return NotFound();

            var stored = user.pass_hash ?? string.Empty;
            if (string.IsNullOrEmpty(stored)) return Ok(new { needsRegistration = true });

            var providedHash = ComputeHash(password ?? string.Empty);
            if (providedHash == stored) return Ok(new { success = true });
            return Unauthorized();
        }
    }
}
