using Inventra.Domain.Exceptions;

namespace Inventra.Domain.Entities
{
    public class CustomIdFormat
    {
        public int Id { get; set; }
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public int InventoryId { get; set; }
        public Inventory Inventory { get; set; } = null!;
        public ICollection<CustomIdElement> Elements { get; set; } = new List<CustomIdElement>();

        public void AddElement(CustomIdElement element)
        {
            if (element == null) throw new DomainException("Element cannot be null");
            Elements.Add(element);
            UpdatedAt = DateTime.UtcNow;
        }

        public void ReplaceElements(IEnumerable<CustomIdElement> newElements)
        {
            Elements.Clear();
            foreach (var element in newElements)
                Elements.Add(element);
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
