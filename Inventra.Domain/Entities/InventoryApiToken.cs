namespace Inventra.Domain.Entities
{
    public class InventoryApiToken
    {
        public int Id { get; set; }
        public int InventoryId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public Inventory Inventory { get; set; } = null!;
    }
}
