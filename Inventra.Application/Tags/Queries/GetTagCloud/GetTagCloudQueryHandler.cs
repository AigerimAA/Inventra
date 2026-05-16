using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Tags.Queries.GetTagCloud
{
    public class GetTagCloudQueryHandler : IRequestHandler<GetTagCloudQuery, IEnumerable<TagCloudItemDto>>
    {
        private readonly ITagRepository _tagRepository;
        public GetTagCloudQueryHandler(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<IEnumerable<TagCloudItemDto>> Handle(GetTagCloudQuery request, CancellationToken cancellationToken)
        {
            var tags = await _tagRepository.GetTagsWithCountAsync(request.MaxTags, cancellationToken);
            return tags.Select(t => new TagCloudItemDto { Name = t.Name, Count = t.Count });
        }
    }
}