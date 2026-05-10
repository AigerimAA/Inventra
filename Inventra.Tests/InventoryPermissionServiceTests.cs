using FluentAssertions;
using Inventra.Domain.Entities;
using Inventra.Infrastructure.Persistence;
using Inventra.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Tests
{
    public class InventoryPermissionServiceTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private async Task<(AppDbContext Context, int InventoryId)> CreateContextWithInventory(
            string ownerId, bool isPublic = false)
        {
            var context = CreateInMemoryContext();
            var inventory = new Inventory("Test", 1, ownerId);
            if (isPublic) inventory.MakePublic();
            inventory.Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }; 
            context.Inventories.Add(inventory);
            await context.SaveChangesAsync();
            return (context, inventory.Id);
        }

        [Fact]
        public async Task CanManageAsync_Admin_ReturnsTrue()
        {
            var context = CreateInMemoryContext();
            var service = new InventoryPermissionService(context);

            var result = await service.CanManageAsync("any-user", isAdmin: true, inventoryId: 999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanManageAsync_Owner_ReturnsTrue()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-123");
            var service = new InventoryPermissionService(context);

            var result = await service.CanManageAsync("owner-123", isAdmin: false, inventoryId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanManageAsync_NotOwner_ReturnFalse()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-123");
            var service = new InventoryPermissionService(context);

            var result = await service.CanManageAsync("other-user", isAdmin: false, inventoryId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanManageAsync_NonExistentInventory_ReturnsFalse()
        {
            var context = CreateInMemoryContext();
            var service = new InventoryPermissionService(context);

            var result = await service.CanManageAsync("any-user", isAdmin: false, inventoryId: 999);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanWriteAsync_Admin_ReturnsTrue()
        {
            var context = CreateInMemoryContext();
            var service = new InventoryPermissionService(context);

            var result = await service.CanWriteAsync("any-user", isAdmin: true, inventoryId: 999);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanWriteAsync_Owner_ReturnsTrue()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-456");
            var service = new InventoryPermissionService(context);

            var result = await service.CanWriteAsync("owner-456", isAdmin: false, inventoryId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanWriteAsync_PublicInventory_ReturnsTrue()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-789", isPublic: true);
            var service = new InventoryPermissionService(context);

            var result = await service.CanWriteAsync("random-user", isAdmin: false, inventoryId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanWriteAsync_UserWithAccess_ReturnsTrue()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-111", isPublic: false);

            context.InventoryAccesses.Add(new InventoryAccess
            {
                InventoryId = inventoryId,
                UserId = "guest-user"
            });
            await context.SaveChangesAsync();

            var service = new InventoryPermissionService(context);
            var result = await service.CanWriteAsync("guest-user", isAdmin: false, inventoryId);

            result.Should().BeTrue();
        }

        [Fact]
        public async Task CanWriteAsync_UnauthorizedUser_ReturnsFalse()
        {
            var (context, inventoryId) = await CreateContextWithInventory("owner-222", isPublic: false);
            var service = new InventoryPermissionService(context);

            var result = await service.CanWriteAsync("stranger", isAdmin: false, inventoryId);

            result.Should().BeFalse();
        }

        [Fact]
        public async Task CanWriteAsync_NonExistentInventory_ReturnsFalse()
        {
            var context = CreateInMemoryContext();
            var service = new InventoryPermissionService(context);

            var result = await service.CanWriteAsync("any-user", isAdmin: false, inventoryId: 999);

            result.Should().BeFalse();
        }
    }
}