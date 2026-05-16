using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Commands.CreateInventory
{
    public class CreateInventoryCommandHandler : IRequestHandler<CreateInventoryCommand, InventoryDto>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;

        public CreateInventoryCommandHandler(IInventoryRepository inventoryRepository, IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUserService)
        {
            _inventoryRepository = inventoryRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
        }

        public async Task<InventoryDto> Handle(CreateInventoryCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException("User is not authenticated");
            var inventory = new Inventory(
                request.Title,
                request.CategoryId,
                userId,
                request.Description,
                request.ImageUrl,
                request.IsPublic);

            await _inventoryRepository.AddAsync(inventory);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            return _mapper.Map<InventoryDto>(inventory);
        }
    }
}
