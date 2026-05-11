using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using Inventra.Domain.ValueObjects;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Repositories
{
    public class AccessRepository : IAccessRepository
    {
        private readonly AppDbContext _context;
        public AccessRepository(AppDbContext context) => _context = context;

        public async Task<IEnumerable<InventoryAccess>> GetUsersWithAccessAsync(int inventoryId, CancellationToken cancellationToken = default)
            => await _context.InventoryAccesses
                .AsNoTracking()
                .Where(a => a.InventoryId == inventoryId)
                .Include(a => a.User)
                .ToListAsync(cancellationToken);

        public async Task<InventoryAccess> AddAccessAsync(int inventoryId, string userId, CancellationToken cancellationToken = default)
        {
            var exists = await _context.InventoryAccesses
                .AnyAsync(a => a.InventoryId == inventoryId && a.UserId == userId, cancellationToken);
            if (exists)
                throw new InvalidOperationException("User already has access");

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken)
                ?? throw new InvalidOperationException("User not found");

            var access = new InventoryAccess { InventoryId = inventoryId, UserId = userId };
            _context.InventoryAccesses.Add(access);
            access.User = user;

            return access;
        }

        public async Task RemoveAccessAsync(int inventoryId, string userId, CancellationToken cancellationToken = default)
        {
            var access = await _context.InventoryAccesses
                .FirstOrDefaultAsync(a => a.InventoryId == inventoryId && a.UserId == userId, cancellationToken);
            if (access != null)
                _context.InventoryAccesses.Remove(access);
        }

        public async Task<IEnumerable<UserLookup>> SearchUsersAsync(string query, CancellationToken cancellationToken = default)
        {
            var raw = await _context.Users
                .AsNoTracking()
                .Where(u => EF.Functions.Like(u.UserName!, $"%{query}%")
                         || EF.Functions.Like(u.Email!, $"%{query}%"))
                .Take(10)
                .Select(u => new { u.Id, UserName = u.UserName!, Email = u.Email! })
                .ToListAsync(cancellationToken);

            return raw.Select(u => new UserLookup(u.Id, u.UserName, u.Email));
        }
    }
}
