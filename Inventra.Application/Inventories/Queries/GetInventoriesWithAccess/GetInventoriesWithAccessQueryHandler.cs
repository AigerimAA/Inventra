using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetInventoriesWithAccess
{
    public class GetInventoriesWithAccessQueryHandler : IRequestHandler<GetInventoriesWithAccessQuery, IEnumerable<InventoryDto>>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IMapper _mapper;

        public GetInventoriesWithAccessQueryHandler(IInventoryRepository inventoryRepository, IMapper mapper)
        {
            _inventoryRepository = inventoryRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<InventoryDto>> Handle(GetInventoriesWithAccessQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _inventoryRepository.GetWithAccessByUserIdAsync(request.UserId, cancellationToken);
            return _mapper.Map<IEnumerable<InventoryDto>>(inventories);
        }
    }
}
