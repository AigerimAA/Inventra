using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetLatestInventories
{
    public class GetLatestInventoriesQueryHandler : IRequestHandler<GetLatestInventoriesQuery, IEnumerable<InventoryDto>>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetLatestInventoriesQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<InventoryDto>> Handle(GetLatestInventoriesQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryRepository.GetLatestAsync(request.Count, cancellationToken);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }
    }
}
