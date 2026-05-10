using FluentAssertions;
using Inventra.Domain.Entities;
using Inventra.Infrastructure.Options;
using Inventra.Infrastructure.Persistence;
using Inventra.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Inventra.Tests
{
    public class SearchRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        private SearchRepository CreateRepo(AppDbContext context, bool useFullText = false)
        {
            var options = Options.Create(new SearchOptions { UseFullText = useFullText });
            return new SearchRepository(context, options, NullLogger<SearchRepository>.Instance);
        }

        [Fact]
        public async Task SearchInventoriesAsync_EmptyQuery_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();
            var repo = CreateRepo(context);

            var result = await repo.SearchInventoriesAsync("");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchInventoriesAsync_WhitespaceQuery_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();
            var repo = CreateRepo(context);

            var result = await repo.SearchInventoriesAsync("   ");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchInventoriesAsync_NoMatch_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();

            var inventory = new Inventory("Books", 1, "user1");
            context.Inventories.Add(inventory);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);
            var result = await repo.SearchInventoriesAsync("xyz123");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchItemsAsync_EmptyQuery_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();
            var repo = CreateRepo(context);

            var result = await repo.SearchItemsAsync("");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchItemsAsync_NoMatch_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();

            var inventory = new Inventory("Test", 1, "user1");
            context.Inventories.Add(inventory);
            await context.SaveChangesAsync();

            var item = new Item(inventory.Id, "user1", "TEST-001");
            item.UpdateValues(
                "Apple", null, null,
                null, null, null,
                null, null, null,
                null, null, null,
                null, null, null,
                null);
            context.Items.Add(item);
            await context.SaveChangesAsync();

            var repo = CreateRepo(context);
            var result = await repo.SearchItemsAsync("xyz123");

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchInventoriesAsync_NullQuery_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();
            var repo = CreateRepo(context);

            var result = await repo.SearchInventoriesAsync(null!);

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task SearchItemsAsync_NullQuery_ReturnsEmpty()
        {
            var context = CreateInMemoryContext();
            var repo = CreateRepo(context);

            var result = await repo.SearchItemsAsync(null!);

            result.Should().BeEmpty();
        }
    }
}