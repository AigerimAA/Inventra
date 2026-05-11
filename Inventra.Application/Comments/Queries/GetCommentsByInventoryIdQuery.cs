using Inventra.Application.DTOs;
using MediatR;

namespace Inventra.Application.Comments.Queries
{
    public record GetCommentsByInventoryIdQuery(int InventoryId) : IRequest<IEnumerable<CommentDto>>;
}
