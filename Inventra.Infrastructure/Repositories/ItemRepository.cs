using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Repositories
{
    public class ItemRepository : IItemRepository
    {
        private readonly AppDbContext _context;

        public ItemRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Item?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            return await _context.Items
                .Include(i => i.CreatedBy)
                .Include(i => i.Likes)
                .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
        }
        public async Task<IEnumerable<Item>> GetByInventoryIdAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            return await _context.Items
                .AsNoTracking()
                .Include(i => i.CreatedBy)
                .Include(i => i.Likes)
                .Where(i => i.InventoryId == inventoryId)
                .ToListAsync(cancellationToken);
        }
        public async Task AddAsync(Item item, CancellationToken cancellationToken = default)
        {
            await _context.Items.AddAsync(item, cancellationToken);
        }
        public Task UpdateAsync(Item item, CancellationToken cancellationToken = default)
        {
            _context.Items.Update(item);
            return Task.CompletedTask;
        }
        public Task SetOriginalVersionAsync(Item item, byte[] expectedVersion, CancellationToken cancellationToken = default)
        {
            _context.Entry(item).Property(i => i.Version).OriginalValue = expectedVersion;
            return Task.CompletedTask;
        }
        public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)
        {
            var item = await _context.Items.FindAsync(id, cancellationToken);
            if (item is not null)
                _context.Items.Remove(item);
        }
    }
}
