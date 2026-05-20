using Inventra.Application.Interfaces;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Services
{
    public class InventoryPermissionService : IInventoryPermissionService
    {
        private readonly AppDbContext _context;

        public InventoryPermissionService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<bool> CanWriteAsync(string userId, bool isAdmin, int inventoryId)
        {
            if (isAdmin) return true;

            return await _context.Inventories
                .AsNoTracking()
                .Where(i => i.Id == inventoryId)
                .AnyAsync(i =>
                    i.OwnerId == userId || i.IsPublic || _context.InventoryAccesses
                        .Any(a => a.InventoryId == i.Id && a.UserId == userId));
        }

        public async Task<bool> CanManageAsync(string userId, bool isAdmin, int inventoryId)
        {
            if (isAdmin) return true;

            return await _context.Inventories
                .AsNoTracking()
                .AnyAsync(i => i.Id == inventoryId && i.OwnerId == userId);
        }
    }
}
