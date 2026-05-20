using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Inventra.Application.Common.Exceptions;
using Inventra.Domain.Entities;
using Inventra.Domain.Interfaces;
using MediatR;

namespace Inventra.Application.Inventories.Queries.ExportInventoryToCsv
{
    public class ExportInventoryToCsvQueryHandler : IRequestHandler<ExportInventoryToCsvQuery, ExportResult>
    {
        private readonly IInventoryRepository _inventoryRepository;
        private readonly IItemRepository _itemRepository;

        public ExportInventoryToCsvQueryHandler(IInventoryRepository inventoryRepository, IItemRepository itemRepository)
        {
            _inventoryRepository = inventoryRepository;
            _itemRepository = itemRepository;
        }

        public async Task<ExportResult> Handle(ExportInventoryToCsvQuery request, CancellationToken cancellationToken)
        {
            var inventory = await _inventoryRepository.GetByIdAsync(request.InventoryId, cancellationToken);
            if (inventory == null) throw new NotFoundException(nameof(Inventory), request.InventoryId);
            var items = await _itemRepository.GetByInventoryIdAsync(request.InventoryId, cancellationToken);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Encoding = Encoding.UTF8
            };

            using var ms = new MemoryStream();
            using var writer = new StreamWriter(ms, Encoding.UTF8);
            using var csv = new CsvWriter(writer, config);

            csv.WriteField("CustomId");
            csv.WriteField("CreatedAt");
            csv.WriteField("CreatedBy");

            if (!string.IsNullOrEmpty(inventory.CustomString1Name)) csv.WriteField(inventory.CustomString1Name);
            if (!string.IsNullOrEmpty(inventory.CustomString2Name)) csv.WriteField(inventory.CustomString2Name);
            if (!string.IsNullOrEmpty(inventory.CustomString3Name)) csv.WriteField(inventory.CustomString3Name);
            if (!string.IsNullOrEmpty(inventory.CustomInt1Name)) csv.WriteField(inventory.CustomInt1Name);
            if (!string.IsNullOrEmpty(inventory.CustomInt2Name)) csv.WriteField(inventory.CustomInt2Name);
            if (!string.IsNullOrEmpty(inventory.CustomInt3Name)) csv.WriteField(inventory.CustomInt3Name);
            if (!string.IsNullOrEmpty(inventory.CustomBool1Name)) csv.WriteField(inventory.CustomBool1Name);
            if (!string.IsNullOrEmpty(inventory.CustomBool2Name)) csv.WriteField(inventory.CustomBool2Name);
            if (!string.IsNullOrEmpty(inventory.CustomBool3Name)) csv.WriteField(inventory.CustomBool3Name);
            if (!string.IsNullOrEmpty(inventory.CustomText1Name)) csv.WriteField(inventory.CustomText1Name);
            if (!string.IsNullOrEmpty(inventory.CustomText2Name)) csv.WriteField(inventory.CustomText2Name);
            if (!string.IsNullOrEmpty(inventory.CustomText3Name)) csv.WriteField(inventory.CustomText3Name);
            if (!string.IsNullOrEmpty(inventory.CustomLink1Name)) csv.WriteField(inventory.CustomLink1Name);
            if (!string.IsNullOrEmpty(inventory.CustomLink2Name)) csv.WriteField(inventory.CustomLink2Name);
            if (!string.IsNullOrEmpty(inventory.CustomLink3Name)) csv.WriteField(inventory.CustomLink3Name);

            await csv.NextRecordAsync();

            foreach (var item in items)
            {
                csv.WriteField(item.CustomId);
                csv.WriteField(item.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                csv.WriteField(item.CreatedBy?.UserName ?? "");

                if (!string.IsNullOrEmpty(inventory.CustomString1Name)) csv.WriteField(item.CustomString1Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomString2Name)) csv.WriteField(item.CustomString2Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomString3Name)) csv.WriteField(item.CustomString3Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomInt1Name)) csv.WriteField(item.CustomInt1Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomInt2Name)) csv.WriteField(item.CustomInt2Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomInt3Name)) csv.WriteField(item.CustomInt3Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomBool1Name)) csv.WriteField(item.CustomBool1Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomBool2Name)) csv.WriteField(item.CustomBool2Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomBool3Name)) csv.WriteField(item.CustomBool3Value?.ToString() ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomText1Name)) csv.WriteField(item.CustomText1Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomText2Name)) csv.WriteField(item.CustomText2Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomText3Name)) csv.WriteField(item.CustomText3Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomLink1Name)) csv.WriteField(item.CustomLink1Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomLink2Name)) csv.WriteField(item.CustomLink2Value ?? "");
                if (!string.IsNullOrEmpty(inventory.CustomLink3Name)) csv.WriteField(item.CustomLink3Value ?? "");

                await csv.NextRecordAsync();
            }
            await writer.FlushAsync();
            var fileName = $"{inventory.Title.Replace(" ", "_")}_{DateTime.UtcNow:yyyyMMdd}.csv";
            return new ExportResult(ms.ToArray(), fileName, "text/csv");
        }
    }
}
