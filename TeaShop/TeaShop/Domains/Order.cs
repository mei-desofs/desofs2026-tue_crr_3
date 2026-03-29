using TeaShop.Enums;

namespace TeaShop.Domains
{
    public class Order(Guid externalId, int userId, OrderStatus status, decimal totalAmount, string? shippingAddress, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public int UserId { get; private set; } = userId;
        public OrderStatus Status { get; private set; } = status;
        public decimal TotalAmount { get; private set; } = totalAmount;
        public string? ShippingAddress { get; private set; } = shippingAddress;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;

        public User User { get; private set; } = null!;
        public ICollection<OrderItem> OrderItems { get; private set; } = [];
        public Payment? Payment { get; private set; }

        public static Order Create(Guid externalId, int userId, OrderStatus status, decimal totalAmount, string? shippingAddress, string? createdBy, string? updatedBy, DateTime? updatedAt)
        {
            return new Order(externalId, userId, status, totalAmount, shippingAddress, createdBy, DateTime.UtcNow, updatedBy, updatedAt);
        }
    }
}
