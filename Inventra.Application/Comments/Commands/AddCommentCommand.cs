using MediatR;

namespace Inventra.Application.Comments.Commands
{
    public record AddCommentCommand(int InventoryId, string Content) : IRequest;
}
