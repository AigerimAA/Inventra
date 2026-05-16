using AutoMapper;
using MediatR;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;

namespace Inventra.Application.Inventories.Queries.GetPopularInventories
{
    public class GetPopularInventoriesQueryHandler : IRequestHandler<GetPopularInventoriesQuery, IEnumerable<InventoryDto>>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetPopularInventoriesQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<InventoryDto>> Handle(GetPopularInventoriesQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryRepository.GetMostPopularAsync(request.Count, cancellationToken);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }
    }
}
