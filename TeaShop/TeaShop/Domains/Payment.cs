using TeaShop.Enums;

namespace TeaShop.Domains
{
    public class Payment(Guid externalId, int orderId, decimal amount, PaymentMethod method, PaymentStatus status, string? transactionId, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public int OrderId { get; private set; } = orderId;
        public decimal Amount { get; private set; } = amount;
        public PaymentMethod Method { get; private set; } = method;
        public PaymentStatus Status { get; private set; } = status;
        public string? TransactionId { get; private set; } = transactionId;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;

        public Order Order { get; private set; } = null!;

        public static Payment Create(Guid externalId, int orderId, decimal amount, PaymentMethod method, PaymentStatus status, string? transactionId, string? createdBy, string? updatedBy, DateTime? updatedAt)
        {
            return new Payment(externalId, orderId, amount, method, status, transactionId, createdBy, DateTime.UtcNow, updatedBy, updatedAt);
        }
    }
}
