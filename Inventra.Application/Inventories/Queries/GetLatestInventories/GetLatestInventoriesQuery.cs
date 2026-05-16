using Inventra.Application.DTOs;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetLatestInventories
{
    public record GetLatestInventoriesQuery(int Count = 10) : IRequest<IEnumerable<InventoryDto>>;
}
