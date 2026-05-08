using FluentAssertions;
using Inventra.Domain.Entities;
using Inventra.Infrastructure.Persistence;
using Inventra.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Tests
{
    public class InventoryStatsRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }
        private Item CreateItem(int inventoryId, decimal? int1 = null, string? str1 = null)
        {
            return new Item
            {
                InventoryId = inventoryId,
                CustomInt1Value = int1,
                CustomString1Value = str1,
                Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }
            };
        }
        [Fact]
        public async Task GetStatsAsync_EmptyInventory_ReturnsTotalItemsZero()
        {
            var context = CreateInMemoryContext();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(1);

            result.TotalItems.Should().Be(0);
            result.Int1Avg.Should().BeNull();
        }

        [Fact]
        public async Task GetStatsAsync_WithItems_ReturnsTotalItemsCount()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 1, InventoryId = 1, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 2, InventoryId = 1, CustomInt1Value = 200, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 3, InventoryId = 1, CustomInt1Value = 300, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
                );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(1);

            result.TotalItems.Should().Be(3);
        }

        [Fact]
        public async Task GetStatsAsync_WithIntValues_CalculatesAverageCorrectly()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 4, InventoryId = 2, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 5, InventoryId = 2, CustomInt1Value = 200, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 6, InventoryId = 2, CustomInt1Value = 300, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(2);

            result.Int1Avg.Should().Be(200);
        }

        [Fact]
        public async Task GetStatsAsync_WithIntValues_ReturnsMinAndMax()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 7, InventoryId = 3, CustomInt1Value = 50, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 8, InventoryId = 3, CustomInt1Value = 150, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 9, InventoryId = 3, CustomInt1Value = 250, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(3);

            result.Int1Min.Should().Be(50);
            result.Int1Max.Should().Be(250);
        }

        [Fact]
        public async Task GetStatsAsync_WithStringValues_ReturnsMostFrequent()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 10, InventoryId = 4, CustomString1Value = "red", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 11, InventoryId = 4, CustomString1Value = "blue", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 12, InventoryId = 4, CustomString1Value = "red", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 13, InventoryId = 4, CustomString1Value = "red", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(4);

            result.String1TopValue.Should().Be("red");
            result.String1TopCount.Should().Be(3);
        }

        [Fact]
        public async Task GetStatsAsync_OnlyCountsItemsForGivenInventory()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 14, InventoryId = 5, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 15, InventoryId = 5, CustomInt1Value = 200, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 16, InventoryId = 99, CustomInt1Value = 999, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(5);

            result.TotalItems.Should().Be(2);
            result.Int1Max.Should().Be(200);
        }

        [Fact]
        public async Task GetStatsAsync_AverageIsRoundedToTwoDecimals()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 17, InventoryId = 6, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 18, InventoryId = 6, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 19, InventoryId = 6, CustomInt1Value = 101, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(6);

            result.Int1Avg.Should().Be(100.33m);
        }

        [Fact]
        public async Task GetStatsAsync_NullValues_AreIgnoredInAverage()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 20, InventoryId = 7, CustomInt1Value = 100, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 21, InventoryId = 7, CustomInt1Value = null, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 22, InventoryId = 7, CustomInt1Value = 300, Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(7);

            result.Int1Avg.Should().Be(200);
        }

        [Fact]
        public async Task GetStatsAsync_EmptyStrings_AreIfnoredInTopValue()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                new Item { Id = 23, InventoryId = 8, CustomString1Value = "", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 24, InventoryId = 8, CustomString1Value = "red", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } },
                new Item { Id = 25, InventoryId = 8, CustomString1Value = "red", Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 } }
            );
            await context.SaveChangesAsync();
            var repo = new InventoryStatsRepository(context);

            var result = await repo.GetStatsAsync(8);

            result.String1TopValue.Should().Be("red");
            result.String1TopCount.Should().Be(2);
        }
    }
}
