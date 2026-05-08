using AutoMapper;
using FluentAssertions;
using Inventra.Application.Common.Mappings;
using Inventra.Application.Interfaces;
using Inventra.Application.Inventories.Commands.CreateInventory;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Inventra.Tests
{
    public class CreateInventoryCommandHandlerTests
    {
        private readonly Mock<IInventoryRepository> _inventoryRepoMock = new();
        private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
        private readonly IMapper _mapper;

        public CreateInventoryCommandHandlerTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();
        }

        [Fact]
        public async Task Handle_ValidRequest_CreatesInventory()
        {
            var command = new CreateInventoryCommand
            {
                Title = "Test Inventory",
                Description = "Test Description",
                IsPublic = true,
                OwnerId = "user-123",
                CategoryId = 1
            };
            _inventoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Inventory>()))
            .Returns(Task.CompletedTask);

            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateInventoryCommandHandler(
                _inventoryRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapper);

            var result = await handler.Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Title.Should().Be("Test Inventory");
            result.OwnerId.Should().Be("user-123");
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsRepositoryAddAsync()
        {
            var command = new CreateInventoryCommand
            {
                Title = "Test",
                OwnerId = "user-123",
                CategoryId = 1
            };
            _inventoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Inventory>()))
            .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateInventoryCommandHandler(
                _inventoryRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapper);

            await handler.Handle(command, CancellationToken.None);

            _inventoryRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Inventory>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsSaveChanges()
        {
            var command = new CreateInventoryCommand
            {
                Title = "Test",
                OwnerId = "user-123",
                CategoryId = 1
            };

            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateInventoryCommandHandler(
                _inventoryRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapper);

            await handler.Handle(command, CancellationToken.None);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_SetsCreatedAtAndUpdatedAt()
        {
            var before = DateTime.UtcNow;
            var command = new CreateInventoryCommand
            {
                Title = "Test",
                OwnerId = "user-123",
                CategoryId = 1
            };

            Inventory? capturedInventory = null;
            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>()))
                .Callback<Inventory>(inv => capturedInventory = inv)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var handler = new CreateInventoryCommandHandler(
                _inventoryRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapper);

            await handler.Handle(command, CancellationToken.None);

            capturedInventory.Should().NotBeNull();
            capturedInventory!.CreatedAt.Should().BeOnOrAfter(before);
            capturedInventory.UpdatedAt.Should().BeOnOrAfter(before);
        }
    }
}
