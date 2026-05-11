using Inventra.Domain.Exceptions;

namespace Inventra.Domain.Entities
{
    public class Comment
    {
        public int Id { get; private set; }
        public string Content { get; private set; } = string.Empty;
        public DateTime CreatedAt { get; private set; }

        public int InventoryId { get; private set; }
        public string AuthorId { get; private set; } = string.Empty;
        public Inventory Inventory { get; set; } = null!;
        public ApplicationUser Author { get; set; } = null!;

        public Comment(int inventoryId, string authorId, string content)
        {
            if (inventoryId <= 0)
                throw new DomainException("InventoryId is required");
            if (string.IsNullOrWhiteSpace(authorId))
                throw new DomainException("AuthorId is required");
            if (string.IsNullOrWhiteSpace(content))
                throw new DomainException("Content cannot be empty");

            InventoryId = inventoryId;
            AuthorId = authorId;
            Content = content;
            CreatedAt = DateTime.UtcNow;
        }

        protected Comment() { }
    }
}

