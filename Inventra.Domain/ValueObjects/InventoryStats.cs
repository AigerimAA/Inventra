namespace Inventra.Domain.ValueObjects
{
    public class InventoryStats
    {
        public int TotalItems { get; }
        public decimal? Int1Avg { get; }
        public decimal? Int1Min { get; }
        public decimal? Int1Max { get; }
        public decimal? Int2Avg { get; }
        public decimal? Int2Min { get; }
        public decimal? Int2Max { get; }
        public decimal? Int3Avg { get; }
        public decimal? Int3Min { get; }
        public decimal? Int3Max { get; }
        public string? String1TopValue { get; }
        public int String1TopCount { get; }
        public string? String2TopValue { get; }
        public int String2TopCount { get; }
        public string? String3TopValue { get; }
        public int String3TopCount { get; }

        public InventoryStats(int totalItems,
            decimal? int1Avg, decimal? int1Min, decimal? int1Max,
            decimal? int2Avg, decimal? int2Min, decimal? int2Max,
            decimal? int3Avg, decimal? int3Min, decimal? int3Max,
            string? string1TopValue, int string1TopCount,
            string? string2TopValue, int string2TopCount,
            string? string3TopValue, int string3TopCount)
        {
            TotalItems = totalItems;
            Int1Avg = int1Avg; Int1Min = int1Min; Int1Max = int1Max;
            Int2Avg = int2Avg; Int2Min = int2Min; Int2Max = int2Max;
            Int3Avg = int3Avg; Int3Min = int3Min; Int3Max = int3Max;
            String1TopValue = string1TopValue; String1TopCount = string1TopCount;
            String2TopValue = string2TopValue; String2TopCount = string2TopCount;
            String3TopValue = string3TopValue; String3TopCount = string3TopCount;
        }
    }
}
