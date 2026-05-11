using System.ComponentModel.DataAnnotations;
using Inventra.Domain.Exceptions;

namespace Inventra.Domain.Entities
{
    public class Inventory
    {
        public int Id { get; private set; }
        public string Title { get; private set; } = string.Empty;

        public string? Description { get; private set; }
        public string? ImageUrl { get; private set; }

        public bool IsPublic { get; private set; } = false;

        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public byte[] Version { get; set; } = null!; 

        public string OwnerId { get; private set; } = string.Empty;
        public int CategoryId { get; private set; }

        public string? CustomString1Name { get; private set; }
        public bool CustomString1Shown { get; private set; }
        public string? CustomString2Name { get; private set; }
        public bool CustomString2Shown { get; private set; }
        public string? CustomString3Name { get; private set; }
        public bool CustomString3Shown { get; private set; }

        public string? CustomInt1Name { get; private set; }
        public bool CustomInt1Shown { get; private set; }
        public string? CustomInt2Name { get; private set; }
        public bool CustomInt2Shown { get; private set; }
        public string? CustomInt3Name { get; private set; }
        public bool CustomInt3Shown { get; private set; }

        public string? CustomText1Name { get; private set; }
        public bool CustomText1Shown { get; private set; }
        public string? CustomText2Name { get; private set; }
        public bool CustomText2Shown { get; private set; }
        public string? CustomText3Name { get; private set; }
        public bool CustomText3Shown { get; private set; }

        public string? CustomBool1Name { get; private set; }
        public bool CustomBool1Shown { get; private set; }
        public string? CustomBool2Name { get; private set; }
        public bool CustomBool2Shown { get; private set; }
        public string? CustomBool3Name { get; private set; }
        public bool CustomBool3Shown { get; private set; }

        public string? CustomLink1Name { get; private set; }
        public bool CustomLink1Shown { get; private set; }
        public string? CustomLink2Name { get; private set; }
        public bool CustomLink2Shown { get; private set; }
        public string? CustomLink3Name { get; private set; }
        public bool CustomLink3Shown { get; private set; }

        public ApplicationUser Owner { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public CustomIdFormat? CustomIdFormat { get; set; }
        public InventorySequence? Sequence { get; set; }

        private readonly List<Item> _items = new();
        private readonly List<InventoryTag> _inventoryTags = new();
        private readonly List<InventoryAccess> _accessList = new();
        private readonly List<Comment> _comments = new();

        public IReadOnlyCollection<Item> Items => _items;
        public IReadOnlyCollection<InventoryTag> InventoryTags => _inventoryTags;
        public IReadOnlyCollection<InventoryAccess> AccessList => _accessList;
        public IReadOnlyCollection<Comment> Comments => _comments;

        public Inventory(string title, int categoryId, string ownerId,
            string? description = null, string? imageUrl = null, bool isPublic = false)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Title cannot be empty");
            if (categoryId <= 0)
                throw new DomainException("Category is required");
            if (string.IsNullOrWhiteSpace(ownerId))
                throw new DomainException("Owner is required");

            Title = title;
            CategoryId = categoryId;
            OwnerId = ownerId;
            Description = description;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        protected Inventory() { }

        public bool CanEdit(string userId, bool isAdmin)
            => isAdmin || OwnerId == userId;

        public bool CanWrite(string userId, bool isAdmin)
            => CanEdit(userId, isAdmin)
               || (IsPublic && !string.IsNullOrWhiteSpace(userId))
               || _accessList.Any(a => a.UserId == userId);

        public void MakePublic()
        {
            IsPublic = true;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MakePrivate()
        {
            IsPublic = false;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDetails(string title, string? description, string? imageUrl, bool isPublic, int categoryId)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("Title cannot be empty");
            if (categoryId <= 0)
                throw new DomainException("Category is required");

            Title = title;
            Description = description;
            ImageUrl = imageUrl;
            IsPublic = isPublic;
            CategoryId = categoryId;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateFields(
            string? str1Name, bool str1Shown, string? str2Name, bool str2Shown, string? str3Name, bool str3Shown,
            string? int1Name, bool int1Shown, string? int2Name, bool int2Shown, string? int3Name, bool int3Shown,
            string? text1Name, bool text1Shown, string? text2Name, bool text2Shown, string? text3Name, bool text3Shown,
            string? bool1Name, bool bool1Shown, string? bool2Name, bool bool2Shown, string? bool3Name, bool bool3Shown,
            string? link1Name, bool link1Shown, string? link2Name, bool link2Shown, string? link3Name, bool link3Shown)
        {
            CustomString1Name = str1Name; CustomString1Shown = str1Shown;
            CustomString2Name = str2Name; CustomString2Shown = str2Shown;
            CustomString3Name = str3Name; CustomString3Shown = str3Shown;

            CustomInt1Name = int1Name; CustomInt1Shown = int1Shown;
            CustomInt2Name = int2Name; CustomInt2Shown = int2Shown;
            CustomInt3Name = int3Name; CustomInt3Shown = int3Shown;

            CustomText1Name = text1Name; CustomText1Shown = text1Shown;
            CustomText2Name = text2Name; CustomText2Shown = text2Shown;
            CustomText3Name = text3Name; CustomText3Shown = text3Shown;

            CustomBool1Name = bool1Name; CustomBool1Shown = bool1Shown;
            CustomBool2Name = bool2Name; CustomBool2Shown = bool2Shown;
            CustomBool3Name = bool3Name; CustomBool3Shown = bool3Shown;

            CustomLink1Name = link1Name; CustomLink1Shown = link1Shown;
            CustomLink2Name = link2Name; CustomLink2Shown = link2Shown;
            CustomLink3Name = link3Name; CustomLink3Shown = link3Shown;

            UpdatedAt = DateTime.UtcNow;
        }

        public void AddTag(Tag tag)
        {
            if (_inventoryTags.Any(t => t.TagId == tag.Id)) return;
            _inventoryTags.Add(new InventoryTag
            {
                TagId = tag.Id,
                InventoryId = Id,
                Tag = tag,
                Inventory = this
            });
        }

        public void ReplaceTags(IEnumerable<Tag> newTags)
        {
            _inventoryTags.Clear();
            foreach (var tag in newTags)
                AddTag(tag);
            UpdatedAt = DateTime.UtcNow;
        }

        public void GrantAccess(string targetUserId)
        {
            if (OwnerId == targetUserId)
                throw new DomainException("Owner already has access as the inventory creator");
            if (_accessList.Any(a => a.UserId == targetUserId))
                return;
            _accessList.Add(new InventoryAccess { InventoryId = Id, UserId = targetUserId });
            UpdatedAt = DateTime.UtcNow;
        }

        public void RevokeAccess(string targetUserId)
        {
            var access = _accessList.FirstOrDefault(a => a.UserId == targetUserId);
            if (access is not null)
            {
                _accessList.Remove(access);
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}