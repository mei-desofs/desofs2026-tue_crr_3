namespace TeaShop.Domains
{
    public class OrderItem(Guid externalId, int orderId, int productId, int quantity, decimal unitPrice, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public int OrderId { get; private set; } = orderId;
        public int ProductId { get; private set; } = productId;
        public int Quantity { get; private set; } = quantity;
        public decimal UnitPrice { get; private set; } = unitPrice;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;

        public Order Order { get; private set; } = null!;
        public Product Product { get; private set; } = null!;

        public static OrderItem Create(Guid externalId, int orderId, int productId, int quantity, decimal unitPrice, string? createdBy, string? updatedBy, DateTime? updatedAt)
        {
            return new OrderItem(externalId, orderId, productId, quantity, unitPrice, createdBy, DateTime.UtcNow, updatedBy, updatedAt);
        }
    }
}
