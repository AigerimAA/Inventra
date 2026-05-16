using AutoMapper;
using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Search.Queries
{
    public class SearchQueryHandler : IRequestHandler<SearchQuery, SearchResultDto>
    {
        private readonly ISearchRepository _searchRepository;
        private readonly IMapper _mapper;

        public SearchQueryHandler(ISearchRepository searchRepository, IMapper mapper)
        {
            _searchRepository = searchRepository;
            _mapper = mapper;
        }

        public async Task<SearchResultDto> Handle(SearchQuery request, CancellationToken cancellationToken)
        {
            var inventories = await _searchRepository.SearchInventoriesAsync(request.Q, cancellationToken);
            var items = await _searchRepository.SearchItemsAsync(request.Q, cancellationToken);

            return new SearchResultDto
            {
                Inventories = _mapper.Map<IEnumerable<InventoryDto>>(inventories),
                Items = items.Select(i => new SearchItemDto
                {
                    Id = i.Id,
                    CustomId = i.CustomId,
                    InventoryId = i.InventoryId,
                    InventoryTitle = i.Inventory?.Title,
                    CreatedByName = i.CreatedBy?.UserName
                })
            };
        }
    }
}
