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
            var item = new Item(inventoryId, "test-user", $"ID-{Guid.NewGuid():N}");
            item.Version = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            item.UpdateValues(
                str1, null, null,
                int1, null, null,
                null, null, null,
                null, null, null,
                null, null, null,
                null);
            return item;
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
                CreateItem(1, int1: 100),
                CreateItem(1, int1: 200),
                CreateItem(1, int1: 300));
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
                CreateItem(2, int1: 100),
                CreateItem(2, int1: 200),
                CreateItem(2, int1: 300));
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
                CreateItem(3, int1: 50),
                CreateItem(3, int1: 150),
                CreateItem(3, int1: 250));
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
                CreateItem(4, str1: "red"),
                CreateItem(4, str1: "blue"),
                CreateItem(4, str1: "red"),
                CreateItem(4, str1: "red"));
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
                CreateItem(5, int1: 100),
                CreateItem(5, int1: 200),
                CreateItem(99, int1: 999));
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
                CreateItem(6, int1: 100),
                CreateItem(6, int1: 100),
                CreateItem(6, int1: 101));
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
                CreateItem(7, int1: 100),
                CreateItem(7, int1: null),
                CreateItem(7, int1: 300));
            await context.SaveChangesAsync();

            var repo = new InventoryStatsRepository(context);
            var result = await repo.GetStatsAsync(7);

            result.Int1Avg.Should().Be(200);
        }

        [Fact]
        public async Task GetStatsAsync_EmptyStrings_AreIgnoredInTopValue()
        {
            var context = CreateInMemoryContext();
            context.Items.AddRange(
                CreateItem(8, str1: ""),
                CreateItem(8, str1: "red"),
                CreateItem(8, str1: "red"));
            await context.SaveChangesAsync();

            var repo = new InventoryStatsRepository(context);
            var result = await repo.GetStatsAsync(8);

            result.String1TopValue.Should().Be("red");
            result.String1TopCount.Should().Be(2);
        }
    }
}