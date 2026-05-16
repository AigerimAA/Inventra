using Inventra.Application.Common.Exceptions;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Commands.DeleteInventory
{
    public class DeleteInventoryCommandHandler : IRequestHandler<DeleteInventoryCommand>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteInventoryCommandHandler(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
        {
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task Handle(DeleteInventoryCommand request, CancellationToken cancellationToken)
        {
            if (!_currentUserService.IsAuthenticated)
                throw new UnauthorizedAccessException("User is not authenticated");

            var inventory = await _inventoryRepository.GetByIdAsync(request.Id)
                ?? throw new NotFoundException(nameof(Inventory), request.Id);

            if (inventory.OwnerId != _currentUserService.UserId && !_currentUserService.IsAdmin)
                throw new UnauthorizedAccessException("Only the inventory owner or an admin can delete this inventory");

            await _inventoryRepository.DeleteAsync(inventory.Id);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}
