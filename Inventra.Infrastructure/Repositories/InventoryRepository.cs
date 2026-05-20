using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly AppDbContext _context;

        public InventoryRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Inventory?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<Inventory>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .Include(i => i.Items)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Inventory>> GetByOwnerIdAsync(string ownerId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .Include(i => i.Items)
                .Where(i => i.OwnerId == ownerId)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Inventory>> GetLatestAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.Items)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .OrderByDescending(i => i.CreatedAt)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Inventory>> GetMostPopularAsync(int count, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.Owner)
                .Include(i => i.Items)
                .OrderByDescending(i => i.Items.Count)
                .Take(count)
                .ToListAsync(cancellationToken);
        }
        public async Task<IEnumerable<Inventory>> GetWithAccessByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Inventories
                .AsNoTracking()
                .Include(i => i.AccessList)
                .Include(i => i.Owner)
                .Include(i => i.Category)
                .Include(i => i.Items)
                .Include(i => i.InventoryTags)
                    .ThenInclude(it => it.Tag)
                .Where(i => i.AccessList.Any(a => a.UserId == userId))
                .ToListAsync(cancellationToken);
        }
        public async Task AddAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            await _context.Inventories.AddAsync(inventory, cancellationToken);
        }
        public Task UpdateAsync(Inventory inventory, CancellationToken cancellationToken = default)
        {
            _context.Inventories.Update(inventory);
            return Task.CompletedTask;
        }
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var inventory = await _context.Inventories.FindAsync(id, cancellationToken);
            if (inventory is not null)
                _context.Inventories.Remove(inventory);
        }
        public void RemoveInventoryTags(IEnumerable<InventoryTag> inventoryTags)
        {
            _context.InventoryTags.RemoveRange(inventoryTags);
        }
        public void SetOriginalVersion(Inventory inventory, byte[] expectedVersion)
        {
            _context.Entry(inventory).Property(i => i.Version).OriginalValue = expectedVersion;
        }
    }
}
