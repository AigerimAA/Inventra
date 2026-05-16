using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Items.Queries.GetItemsByInventoryId
{
    public class GetItemsByInventoryIdQueryHandler : IRequestHandler<GetItemsByInventoryIdQuery, IEnumerable<ItemDto>>
    {
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;

        public GetItemsByInventoryIdQueryHandler(IItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }
        public async Task<IEnumerable<ItemDto>> Handle(GetItemsByInventoryIdQuery request, CancellationToken cancellationToken)
        {
            var items = await _itemRepository.GetByInventoryIdAsync(request.InventoryId, cancellationToken);
            return _mapper.Map<IEnumerable<ItemDto>>(items);
        }
    }
}
