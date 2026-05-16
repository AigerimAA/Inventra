using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetAllInventories
{
    public class GetAllInventoriesQueryHandler : IRequestHandler<GetAllInventoriesQuery, IEnumerable<InventoryDto>>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetAllInventoriesQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<InventoryDto>> Handle(GetAllInventoriesQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryRepository.GetAllAsync(cancellationToken);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }
    }
}
