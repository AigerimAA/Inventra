using Inventra.Application.DTOs;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.GetInventoryStats
{
    public class GetInventoryStatsQueryHandler : IRequestHandler<GetInventoryStatsQuery, InventoryStatsDto>
    {
        private readonly IInventoryStatsRepository _statsRepository;
        public GetInventoryStatsQueryHandler(IInventoryStatsRepository statsRepository)
        {
            _statsRepository = statsRepository;
        }
        public async Task<InventoryStatsDto> Handle(GetInventoryStatsQuery request, CancellationToken cancellationToken)
        {
            var stats = await _statsRepository.GetStatsAsync(request.InventoryId, cancellationToken);
            return new InventoryStatsDto
            {
                TotalItems = stats.TotalItems,
                Int1Avg = stats.Int1Avg,
                Int1Min = stats.Int1Min,
                Int1Max = stats.Int1Max,
                Int2Avg = stats.Int2Avg,
                Int2Min = stats.Int2Min,
                Int2Max = stats.Int2Max,
                Int3Avg = stats.Int3Avg,
                Int3Min = stats.Int3Min,
                Int3Max = stats.Int3Max,
                String1TopValue = stats.String1TopValue,
                String1TopCount = stats.String1TopCount,
                String2TopValue = stats.String2TopValue,
                String2TopCount = stats.String2TopCount,
                String3TopValue = stats.String3TopValue,
                String3TopCount = stats.String3TopCount
            };
        }
    }
}
