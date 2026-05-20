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
        private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();
        private readonly IMapper _mapper;

        public CreateInventoryCommandHandlerTests()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddAutoMapper(cfg => cfg.AddProfile<MappingProfile>());
            var provider = services.BuildServiceProvider();
            _mapper = provider.GetRequiredService<IMapper>();

            _currentUserServiceMock.Setup(s => s.UserId).Returns("user-123");
            _currentUserServiceMock.Setup(s => s.IsAuthenticated).Returns(true);
            _currentUserServiceMock.Setup(s => s.IsAdmin).Returns(false);
        }

        private CreateInventoryCommandHandler CreateHandler() =>
            new CreateInventoryCommandHandler(
                _inventoryRepoMock.Object,
                _unitOfWorkMock.Object,
                _mapper,
                _currentUserServiceMock.Object);

        [Fact]
        public async Task Handle_ValidRequest_CreatesInventory()
        {
            var command = new CreateInventoryCommand
            {
                Title = "Test Inventory",
                Description = "Test Description",
                IsPublic = true,
                CategoryId = 1
            };
            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            var result = await CreateHandler().Handle(command, CancellationToken.None);

            result.Should().NotBeNull();
            result.Title.Should().Be("Test Inventory");
            result.OwnerId.Should().Be("user-123");
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsRepositoryAddAsync()
        {
            var command = new CreateInventoryCommand { Title = "Test", CategoryId = 1 };
            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateHandler().Handle(command, CancellationToken.None);

            _inventoryRepoMock.Verify(
                r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_CallsSaveChanges()
        {
            var command = new CreateInventoryCommand { Title = "Test", CategoryId = 1 };
            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateHandler().Handle(command, CancellationToken.None);

            _unitOfWorkMock.Verify(
                u => u.SaveChangesAsync(It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task Handle_ValidRequest_SetsCreatedAtAndUpdatedAt()
        {
            var before = DateTime.UtcNow;
            var command = new CreateInventoryCommand { Title = "Test", CategoryId = 1 };

            Inventory? capturedInventory = null;
            _inventoryRepoMock
                .Setup(r => r.AddAsync(It.IsAny<Inventory>(), It.IsAny<CancellationToken>()))
                .Callback<Inventory, CancellationToken>((inv, _) => capturedInventory = inv)
                .Returns(Task.CompletedTask);
            _unitOfWorkMock
                .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(1);

            await CreateHandler().Handle(command, CancellationToken.None);

            capturedInventory.Should().NotBeNull();
            capturedInventory!.CreatedAt.Should().BeOnOrAfter(before);
            capturedInventory.UpdatedAt.Should().BeOnOrAfter(before);
        }

        [Fact]
        public async Task Handle_UnauthenticatedUser_ThrowsUnauthorizedException()
        {
            _currentUserServiceMock.Setup(s => s.UserId).Returns((string?)null);

            var command = new CreateInventoryCommand { Title = "Test", CategoryId = 1 };

            var act = async () => await CreateHandler().Handle(command, CancellationToken.None);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }
    }
}