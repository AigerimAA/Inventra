using MediatR;

namespace Inventra.Application.Tags.Queries.GetTagsByPrefix
{
    public record GetTagsByPrefixQuery(string Prefix, int MaxResults = 10)
        : IRequest<IEnumerable<string>>;
}
