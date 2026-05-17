using System.Text;
using Inventra.Application.Interfaces;
using Inventra.Domain.Entities;
using Inventra.Domain.Enums;
using Inventra.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Inventra.Infrastructure.Services
{
    public class CustomIdGeneratorService : ICustomIdGenerator
    {
        private readonly AppDbContext _context;

        public CustomIdGeneratorService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAsync(int inventoryId, CancellationToken cancellationToken = default)
        {
            var format = await _context.CustomIdFormats
                .Include(f => f.Elements)
                .FirstOrDefaultAsync(f => f.InventoryId == inventoryId, cancellationToken);

            if (format == null || !format.Elements.Any())
                return Guid.NewGuid().ToString("N")[..12].ToUpper();

            var orderedElements = format.Elements.OrderBy(e => e.SortOrder).ToList();

            for (int attempt = 0; attempt < 3; attempt++)
            {
                var generated = await BuildIdAsync(orderedElements, inventoryId, cancellationToken);

                var exists = await _context.Items
                    .AnyAsync(i => i.InventoryId == inventoryId
                                && i.CustomId == generated, cancellationToken);

                if (!exists) return generated;
            }

            var fallback = await BuildIdAsync(orderedElements, inventoryId, cancellationToken);
            return fallback + "_" + Guid.NewGuid().ToString("N")[..4].ToUpper();
        }
        private async Task<string> BuildIdAsync(IEnumerable<CustomIdElement> elements, int inventoryId, CancellationToken ct)
        {
            var sb = new StringBuilder();

            foreach (var element in elements)
            {
                var part = element.ElementType == CustomIdElementType.Sequence
                    ? await BuildSequenceAsync(inventoryId, element.FormatString, ct)
                    : BuildSyncElement(element);

                sb.Append(part);
            }

            return sb.ToString();
        }

        private static string BuildSyncElement(CustomIdElement element)
        {
            return element.ElementType switch
            {
                CustomIdElementType.Fixed =>
                    element.FixedValue ?? string.Empty,

                CustomIdElementType.DateTime =>
                    DateTime.UtcNow.ToString(
                        string.IsNullOrEmpty(element.FormatString)
                            ? "yyyy"
                            : element.FormatString),

                CustomIdElementType.Guid =>
                    Guid.NewGuid().ToString("N").ToUpper(),

                CustomIdElementType.Random20 =>
                    FormatHex(
                        Random.Shared.Next(0, 0x100000),
                        element.FormatString,
                        defaultWidth: 5),

                CustomIdElementType.Random32 =>
                    FormatHexLong(
                        Random.Shared.NextInt64(0, 0x100000000L),
                        element.FormatString,
                        defaultWidth: 8),

                CustomIdElementType.Random6Digits =>
                    FormatNumber(
                        Random.Shared.Next(100000, 1000000),
                        element.FormatString,
                        defaultPadding: 6),

                CustomIdElementType.Random9Digits =>
                    FormatNumber(
                        Random.Shared.Next(100000000, 1000000000),
                        element.FormatString,
                        defaultPadding: 9),

                _ => throw new ArgumentException($"Unsupported element type: {element.ElementType}")
            };
        }

        private static string FormatNumber(int value, string? fmt, int defaultPadding)
        {
            if (string.IsNullOrEmpty(fmt))
                return value.ToString().PadLeft(defaultPadding, '0');

            if (fmt.StartsWith('D') && int.TryParse(fmt[1..], out var width))
                return value.ToString().PadLeft(width, '0');

            return value.ToString();
        }

        private static string FormatHex(int value, string? fmt, int defaultWidth)
        {
            if (string.IsNullOrEmpty(fmt))
                return value.ToString("X").PadLeft(defaultWidth, '0');

            if (fmt.StartsWith('X') && int.TryParse(fmt[1..], out var width))
                return value.ToString("X").PadLeft(width, '0');

            return value.ToString("X");
        }
        private static string FormatHexLong(long value, string? fmt, int defaultWidth)
        {
            if (string.IsNullOrEmpty(fmt))
                return value.ToString("X").PadLeft(defaultWidth, '0');

            if (fmt.StartsWith('X') && int.TryParse(fmt[1..], out var width))
                return value.ToString("X").PadLeft(width, '0');

            return value.ToString("X");
        }

        private async Task<string> BuildSequenceAsync(
            int inventoryId, string? fmt, CancellationToken cancellationToken)
        {
            while (true)
            {
                var seq = await _context.InventorySequence
                    .FirstOrDefaultAsync(s => s.InventoryId == inventoryId, cancellationToken);

                if (seq == null)
                {
                    seq = new InventorySequence
                    {
                        InventoryId = inventoryId,
                        CurrentValue = 1
                    };
                    _context.InventorySequence.Add(seq);

                    try
                    {
                        await _context.SaveChangesAsync(cancellationToken);
                        return FormatSequenceValue(1, fmt);
                    }
                    catch (DbUpdateException)
                    {
                        _context.Entry(seq).State = EntityState.Detached;
                        continue;
                    }
                }

                seq.CurrentValue += 1;

                try
                {
                    await _context.SaveChangesAsync(cancellationToken);
                    return FormatSequenceValue(seq.CurrentValue, fmt);
                }
                catch (DbUpdateConcurrencyException)
                {
                    await _context.Entry(seq).ReloadAsync(cancellationToken);
                }
            }
        }

        private static string FormatSequenceValue(int value, string? fmt)
        {
            if (string.IsNullOrEmpty(fmt))
                return value.ToString();

            if (fmt.StartsWith('D') && int.TryParse(fmt[1..], out var width))
                return value.ToString().PadLeft(width, '0');

            return value.ToString();
        }
    }
}
