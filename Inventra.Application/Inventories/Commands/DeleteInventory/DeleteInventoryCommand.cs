using MediatR;

namespace Inventra.Application.Inventories.Commands.DeleteInventory
{
    public record DeleteInventoryCommand(int Id) : IRequest;
}
