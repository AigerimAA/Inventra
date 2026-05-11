using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.CustomId.Queries.GetCustomIdFormat
{
    public class GetCustomIdFormatQueryHandler : IRequestHandler<GetCustomIdFormatQuery, CustomIdFormatDto?>
    {
        private readonly ICustomIdRepository _repository;
        public GetCustomIdFormatQueryHandler(ICustomIdRepository repository)
        {
            _repository = repository;
        }

        public async Task<CustomIdFormatDto?> Handle(GetCustomIdFormatQuery request, CancellationToken cancellationToken = default)
        {
            var format = await _repository.GetByInventoryIdAsync(request.InventoryId, cancellationToken);
            if (format == null) return new CustomIdFormatDto { InventoryId = request.InventoryId };

            return new CustomIdFormatDto
            {
                InventoryId = format.InventoryId,
                Elements = format.Elements
                    .OrderBy(e => e.SortOrder)
                    .Select(e => new CustomIdElementDto
                    {
                        Id = e.Id,
                        ElementType = e.ElementType.ToString("G"),
                        FormatString = e.FormatString,
                        FixedValue = e.FixedValue,
                        SortOrder = e.SortOrder
                    }).ToList()
            };
        }
    }
}
