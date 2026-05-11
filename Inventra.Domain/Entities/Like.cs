using Inventra.Domain.Exceptions;

namespace Inventra.Domain.Entities
{
    public class Like
    {
        public int ItemId { get; private set; }
        public string UserId { get; private set; } = string.Empty;
        public Item Item { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

        public Like(int itemId, string userId)
        {
            if (itemId <= 0)
                throw new DomainException("ItemId is required");
            if (string.IsNullOrWhiteSpace(userId))
                throw new DomainException("UserId is required");

            ItemId = itemId;
            UserId = userId;
        }

        protected Like() { }
    }
}
