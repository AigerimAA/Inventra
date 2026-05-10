using Inventra.Domain.Entities;
using Inventra.Domain.Enums;
using Inventra.Infrastructure.Persistence;
using Inventra.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using FluentAssertions;

namespace Inventra.Tests
{
    public class CustomIdGeneratorTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(options);
        }

        [Fact]
        public async Task GenerateAsync_NoFormat_ReturnsGuidFallback()
        {
            var context = CreateInMemoryContext();
            var service = new CustomIdGeneratorService(context);

            var result = await service.GenerateAsync(999);

            result.Should().NotBeNullOrEmpty();
            result.Should().MatchRegex("^[A-F0-9]{12}$");
        }

        [Fact]
        public async Task GenerateAsync_FixedElement_ReturnsFixedValue()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 1,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "INV-", SortOrder = 0 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(1);

            result.Should().StartWith("INV-");
        }

        [Fact]
        public async Task GenerateAsync_SequenceElement_ReturnsFormattedSequence()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 3,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "INV-", SortOrder = 0 },
                    new() { ElementType = CustomIdElementType.Sequence, FormatString = "D4", SortOrder = 1 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(3);

            result.Should().Be("INV-0001");
        }

        [Fact]
        public async Task GenerateAsync_SequenceElement_Increments()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 4,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Sequence, FormatString = "D3", SortOrder = 0 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);

            var first = await service.GenerateAsync(4);
            var second = await service.GenerateAsync(4);

            first.Should().Be("001");
            second.Should().Be("002");
        }

        [Fact]
        public async Task GenerateAsync_Random6Digits_IsValidRange()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 13,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Random6Digits, SortOrder = 0 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(13);

            result.Should().HaveLength(6);
            result.All(char.IsDigit).Should().BeTrue();
            int.Parse(result).Should().BeInRange(100000, 999999);
        }

        [Fact]
        public async Task GenerateAsync_MultipleElements_ConcatenatesAll()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 6,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "PRD", SortOrder = 0 },
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "-", SortOrder = 1 },
                    new() { ElementType = CustomIdElementType.DateTime, FormatString = "yyyy", SortOrder = 2 },
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "-", SortOrder = 3 },
                    new() { ElementType = CustomIdElementType.Sequence, FormatString = "D4", SortOrder = 4 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(6);

            result.Should().StartWith($"PRD-{DateTime.UtcNow.Year}-");
            result.Should().EndWith("0001");
        }

        [Fact]
        public async Task GenerateAsync_WhenIdExists_GeneratesAnotherOne()
        {
            var context = CreateInMemoryContext();

            context.InventorySequence.Add(new InventorySequence { InventoryId = 10, CurrentValue = 1 });

            var existingItem = new Item(10, "test-user", "INV-0002");
            context.Items.Add(existingItem);

            var format = new CustomIdFormat
            {
                InventoryId = 10,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "INV-", SortOrder = 0 },
                    new() { ElementType = CustomIdElementType.Sequence, FormatString = "D4", SortOrder = 1 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(10);

            result.Should().Be("INV-0003");
        }

        [Fact]
        public async Task GenerateAsync_OrdersElementsBySortOrder()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 11,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.Sequence, FormatString = "D2", SortOrder = 1 },
                    new() { ElementType = CustomIdElementType.Fixed, FixedValue = "INV-", SortOrder = 0 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(11);

            result.Should().Be("INV-01");
        }

        [Fact]
        public async Task GenerateAsync_DateTimeElement_ContainsCurrentYear_Regex()
        {
            var context = CreateInMemoryContext();
            var format = new CustomIdFormat
            {
                InventoryId = 12,
                Elements = new List<CustomIdElement>
                {
                    new() { ElementType = CustomIdElementType.DateTime, FormatString = "yyyy", SortOrder = 0 }
                }
            };
            context.CustomIdFormats.Add(format);
            await context.SaveChangesAsync();

            var service = new CustomIdGeneratorService(context);
            var result = await service.GenerateAsync(12);

            result.Should().MatchRegex(@"^\d{4}$");
            result.Should().Contain(DateTime.UtcNow.Year.ToString());
        }
    }
}