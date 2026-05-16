using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetInventoriesByUserId
{
    public class GetInventoriesByUserIdQueryHandler : IRequestHandler<GetInventoriesByUserIdQuery, IEnumerable<InventoryDto>>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetInventoriesByUserIdQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryDto>> Handle(GetInventoriesByUserIdQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryRepository.GetByOwnerIdAsync(request.UserId, cancellationToken);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }
    }
}
