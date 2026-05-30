using Inventra.Domain.Entities;
using Inventra.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Web.Controllers
{
    [Route("api")]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ApiController(AppDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost("inventory/{inventoryId}/generate-token")]
        public async Task<IActionResult> GenerateToken(int inventoryId)
        {
            var existing = await _context.InventoryApiTokens.FirstOrDefaultAsync(t => t.InventoryId == inventoryId);

            if (existing != null) return Ok(new { token = existing.Token });

            var token = new InventoryApiToken
            {
                InventoryId = inventoryId,
                Token = Guid.NewGuid().ToString("N"),
                CreatedAt = DateTime.UtcNow
            };

            _context.InventoryApiTokens.Add(token);
            await _context.SaveChangesAsync();
            return Ok(new { token = token.Token });
        }

        [HttpGet("inventory")]
        public async Task<IActionResult> GetInventoryData([FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return BadRequest(new { error = "Token is required" });

            var apiToken = await _context.InventoryApiTokens
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Items)
                .Include(t => t.Inventory)
                    .ThenInclude(i => i.Category)
                .FirstOrDefaultAsync(t => t.Token == token);

            if (apiToken == null) return Unauthorized(new { error = "Invalid token" });

            var inv = apiToken.Inventory;
            var items = inv.Items.ToList();
            var fields = new List<object>();

            if (inv.CustomString1Name != null)
                fields.Add(new { title = inv.CustomString1Name, type = "string", topValues = items.Where(i => i.CustomString1Value != null).GroupBy(i => i.CustomString1Value).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key) });
            if (inv.CustomString2Name != null)
                fields.Add(new { title = inv.CustomString2Name, type = "string", topValues = items.Where(i => i.CustomString2Value != null).GroupBy(i => i.CustomString2Value).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key) });
            if (inv.CustomString3Name != null)
                fields.Add(new { title = inv.CustomString3Name, type = "string", topValues = items.Where(i => i.CustomString3Value != null).GroupBy(i => i.CustomString3Value).OrderByDescending(g => g.Count()).Take(3).Select(g => g.Key) });
            if (inv.CustomInt1Name != null)
                fields.Add(new { title = inv.CustomInt1Name, type = "number", avg = items.Where(i => i.CustomInt1Value.HasValue).Average(i => i.CustomInt1Value), min = items.Where(i => i.CustomInt1Value.HasValue).Min(i => i.CustomInt1Value), max = items.Where(i => i.CustomInt1Value.HasValue).Max(i => i.CustomInt1Value) });
            if (inv.CustomInt2Name != null)
                fields.Add(new { title = inv.CustomInt2Name, type = "number", avg = items.Where(i => i.CustomInt2Value.HasValue).Average(i => i.CustomInt2Value), min = items.Where(i => i.CustomInt2Value.HasValue).Min(i => i.CustomInt2Value), max = items.Where(i => i.CustomInt2Value.HasValue).Max(i => i.CustomInt2Value) });
            if (inv.CustomInt3Name != null)
                fields.Add(new { title = inv.CustomInt3Name, type = "number", avg = items.Where(i => i.CustomInt3Value.HasValue).Average(i => i.CustomInt3Value), min = items.Where(i => i.CustomInt3Value.HasValue).Min(i => i.CustomInt3Value), max = items.Where(i => i.CustomInt3Value.HasValue).Max(i => i.CustomInt3Value) });

            return Ok(new
            {
                inventoryTitle = inv.Title,
                category = inv.Category?.Name,
                totalItems = items.Count,
                fields
            });
        }
    }
}
