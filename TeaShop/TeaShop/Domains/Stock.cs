namespace TeaShop.Domains
{
    public class Stock(Guid externalId, int productId, int quantity, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public int ProductId { get; private set; } = productId;
        public int Quantity { get; private set; } = quantity;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;

        public Product Product { get; private set; } = null!;

        public static Stock Create(Guid externalId, int productId, int quantity, string? createdBy, string? updatedBy, DateTime? updatedAt)
        {
            return new Stock(externalId, productId, quantity, createdBy, DateTime.UtcNow, updatedBy, updatedAt);
        }
    }
}
