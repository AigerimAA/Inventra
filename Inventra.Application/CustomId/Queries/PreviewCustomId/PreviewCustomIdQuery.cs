using MediatR;

namespace Inventra.Application.CustomId.Queries.PreviewCustomId
{
    public record PreviewCustomIdQuery(int InventoryId) : IRequest<string>;
}
