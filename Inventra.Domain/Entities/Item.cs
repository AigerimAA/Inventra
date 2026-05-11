using System.ComponentModel.DataAnnotations;
using Inventra.Domain.Exceptions;

namespace Inventra.Domain.Entities
{
    public class Item
    {
        public int Id { get; private set; }
        public string CustomId { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }
        public byte[] Version { get; set; } = null!;

        public int InventoryId { get; private set; }
        public string CreatedById { get; private set; } = string.Empty;

        public string? CustomString1Value { get; private set; }
        public string? CustomString2Value { get; private set; }
        public string? CustomString3Value { get; private set; }

        public decimal? CustomInt1Value { get; private set; }
        public decimal? CustomInt2Value { get; private set; }
        public decimal? CustomInt3Value { get; private set; }

        public string? CustomText1Value { get; private set; }
        public string? CustomText2Value { get; private set; }
        public string? CustomText3Value { get; private set; }

        public bool? CustomBool1Value { get; private set; }
        public bool? CustomBool2Value { get; private set; }
        public bool? CustomBool3Value { get; private set; }

        public string? CustomLink1Value { get; private set; }
        public string? CustomLink2Value { get; private set; }
        public string? CustomLink3Value { get; private set; }

        public string? ImageUrl { get; private set; }

        public Inventory Inventory { get; set; } = null!;
        public ApplicationUser CreatedBy { get; set; } = null!;
        private readonly List<Like> _likes = new();
        public IReadOnlyCollection<Like> Likes => _likes;

        public Item(int inventoryId, string createdById, string customId)
        {
            if (inventoryId <= 0)
                throw new DomainException("InventoryId is required");
            if (string.IsNullOrWhiteSpace(createdById))
                throw new DomainException("CreatedById is required");
            if (string.IsNullOrWhiteSpace(customId))
                throw new DomainException("CustomId cannot be empty");

            InventoryId = inventoryId;
            CreatedById = createdById;
            CustomId = customId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        protected Item() { }

        public void UpdateValues(
            string? str1, string? str2, string? str3, decimal? int1, decimal? int2, decimal? int3,
            string? text1, string? text2, string? text3, bool? bool1, bool? bool2, bool? bool3,
            string? link1, string? link2, string? link3, string? imageUrl)
        {
            CustomString1Value = str1; CustomString2Value = str2; CustomString3Value = str3;
            CustomInt1Value = int1; CustomInt2Value = int2; CustomInt3Value = int3;
            CustomText1Value = text1; CustomText2Value = text2; CustomText3Value = text3;
            CustomBool1Value = bool1; CustomBool2Value = bool2; CustomBool3Value = bool3;
            CustomLink1Value = link1; CustomLink2Value = link2; CustomLink3Value = link3;
            ImageUrl = imageUrl; UpdatedAt = DateTime.UtcNow;
        }

        public void ChangeCustomId(string newCustomId)
        {
            if (string.IsNullOrWhiteSpace(newCustomId))
                throw new DomainException("CustomId cannot be empty");
            CustomId = newCustomId;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}