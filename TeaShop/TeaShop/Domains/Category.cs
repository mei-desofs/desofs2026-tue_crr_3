namespace TeaShop.Domains
{
    public class Category(Guid externalId, string name, string? description, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt, bool active)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public string Name { get; private set; } = name;
        public string? Description { get; private set; } = description;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;
        public bool Active { get; private set; } = active;

        public ICollection<Product> Products { get; private set; } = [];

        public static Category Create(Guid externalId, string name, string? description, string? createdBy, string? updatedBy, DateTime? updatedAt, bool active)
        {
            return new Category(externalId, name, description, createdBy, DateTime.UtcNow, updatedBy, updatedAt, active);
        }
    }
}
