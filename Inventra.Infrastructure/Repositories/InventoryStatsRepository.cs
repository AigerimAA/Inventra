using Inventra.Domain.Interfaces;
using Inventra.Domain.ValueObjects;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Repositories
{
    public class InventoryStatsRepository : IInventoryStatsRepository
    {
        private readonly AppDbContext _context;
        public InventoryStatsRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<InventoryStats> GetStatsAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            var q = _context.Items.AsNoTracking().Where(i => i.InventoryId == inventoryId);
            var totalItems = await q.CountAsync(cancellationToken);

            if (totalItems == 0)
                return new InventoryStats(0, null, null, null, null, null, null, null, null, null,
                    null, 0, null, 0, null, 0);

            var int1Stats = await q
                .Where(i => i.CustomInt1Value.HasValue)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    Avg = g.Average(x => x.CustomInt1Value),
                    Min = g.Min(x => x.CustomInt1Value),
                    Max = g.Max(x => x.CustomInt1Value)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var int2Stats = await q
                .Where(i => i.CustomInt2Value.HasValue)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    Avg = g.Average(x => x.CustomInt2Value),
                    Min = g.Min(x => x.CustomInt2Value),
                    Max = g.Max(x => x.CustomInt2Value)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var int3Stats = await q
                .Where(i => i.CustomInt3Value.HasValue)
                .GroupBy(i => 1)
                .Select(g => new
                {
                    Avg = g.Average(x => x.CustomInt3Value),
                    Min = g.Min(x => x.CustomInt3Value),
                    Max = g.Max(x => x.CustomInt3Value)
                })
                .FirstOrDefaultAsync(cancellationToken);

            var str1Top = await q
                .Where(i => !string.IsNullOrWhiteSpace(i.CustomString1Value))
                .GroupBy(i => i.CustomString1Value)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync(cancellationToken);

            var str2Top = await q
                .Where(i => !string.IsNullOrWhiteSpace(i.CustomString2Value))
                .GroupBy(i => i.CustomString2Value)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync(cancellationToken);

            var str3Top = await q
                .Where(i => !string.IsNullOrWhiteSpace(i.CustomString3Value))
                .GroupBy(i => i.CustomString3Value)
                .Select(g => new { Value = g.Key, Count = g.Count() })
                .OrderByDescending(g => g.Count)
                .FirstOrDefaultAsync(cancellationToken);

            return new InventoryStats(
                totalItems,
                int1Stats?.Avg.HasValue == true ? Math.Round(int1Stats.Avg!.Value, 2) : null,
                int1Stats?.Min,
                int1Stats?.Max,
                int2Stats?.Avg.HasValue == true ? Math.Round(int2Stats.Avg!.Value, 2) : null,
                int2Stats?.Min,
                int2Stats?.Max,
                int3Stats?.Avg.HasValue == true ? Math.Round(int3Stats.Avg!.Value, 2) : null,
                int3Stats?.Min,
                int3Stats?.Max,
                str1Top?.Value, str1Top?.Count ?? 0,
                str2Top?.Value, str2Top?.Count ?? 0,
                str3Top?.Value, str3Top?.Count ?? 0);
        }
    }
}
