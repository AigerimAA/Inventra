using MediatR;
using Inventra.Domain.Interfaces;

namespace Inventra.Application.Tags.Queries.GetTagsByPrefix
{
    public class GetTagsByPrefixQueryHandler : IRequestHandler<GetTagsByPrefixQuery, IEnumerable<string>>
    {
        private readonly ITagRepository _tagRepository;
        public GetTagsByPrefixQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<string>> Handle(GetTagsByPrefixQuery request, CancellationToken cancellationToken)
        {
            var tags = await _tagRepository.GetByPrefixAsync(request.Prefix, request.MaxResults, cancellationToken);
            return tags.Select(t => t.Name);
        }
    }
}
