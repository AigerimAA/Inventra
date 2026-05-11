using Inventra.Application.DTOs;
using MediatR;

namespace Inventra.Application.CustomId.Queries.GetCustomIdFormat
{
    public record GetCustomIdFormatQuery(int InventoryId) : IRequest<CustomIdFormatDto?>;
}
