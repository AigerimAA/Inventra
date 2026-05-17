namespace Inventra.Domain.ValueObjects
{
    public record InventoryStats(
        int TotalItems,
        decimal? Int1Avg, decimal? Int1Min, decimal? Int1Max,
        decimal? Int2Avg, decimal? Int2Min, decimal? Int2Max,
        decimal? Int3Avg, decimal? Int3Min, decimal? Int3Max,
        string? String1TopValue, int String1TopCount,
        string? String2TopValue, int String2TopCount,
        string? String3TopValue, int String3TopCount);
}

