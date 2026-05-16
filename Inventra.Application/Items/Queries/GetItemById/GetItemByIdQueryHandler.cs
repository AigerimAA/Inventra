using AutoMapper;
using Inventra.Application.Common.Exceptions;
using Inventra.Application.DTOs;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Items.Queries.GetItemById
{
    public class GetItemByIdQueryHandler : IRequestHandler<GetItemByIdQuery, ItemDto>
    {
        private readonly IItemRepository _itemRepository;
        private readonly IMapper _mapper;

        public GetItemByIdQueryHandler(IItemRepository itemRepository, IMapper mapper)
        {
            _itemRepository = itemRepository;
            _mapper = mapper;
        }

        public async Task<ItemDto> Handle (GetItemByIdQuery request, CancellationToken cancellationToken)
        {
            var item = await _itemRepository.GetByIdAsync(request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(Item), request.Id);

            return _mapper.Map<ItemDto>(item);
        }
    }
}
