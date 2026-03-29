namespace TeaShop.Domains
{
    public class Product(Guid externalId, string name, string? description, decimal price, int categoryId, string? imageUrl, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt, bool active)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public string Name { get; private set; } = name;
        public string? Description { get; private set; } = description;
        public decimal Price { get; private set; } = price;
        public int CategoryId { get; private set; } = categoryId;
        public string? ImageUrl { get; private set; } = imageUrl;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;
        public bool Active { get; private set; } = active;

        public Category Category { get; private set; } = null!;
        public Stock? Stock { get; private set; }
        public ICollection<OrderItem> OrderItems { get; private set; } = [];

        public static Product Create(Guid externalId, string name, string? description, decimal price, int categoryId, string? imageUrl, string? createdBy, string? updatedBy, DateTime? updatedAt, bool active)
        {
            return new Product(externalId, name, description, price, categoryId, imageUrl, createdBy, DateTime.UtcNow, updatedBy, updatedAt, active);
        }
    }
}
