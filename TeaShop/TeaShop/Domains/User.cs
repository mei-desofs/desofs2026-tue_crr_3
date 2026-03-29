namespace TeaShop.Domains
{
    public class User(Guid externalId, string firstName, string lastName, string email, string? phoneNumber, string? address, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt, bool active)
    {
        public int Id { get; private set; }
        public Guid ExternalId { get; private set; } = externalId;
        public string FirstName { get; private set; } = firstName;
        public string LastName { get; private set; } = lastName;
        public string Email { get; private set; } = email;
        public string? PhoneNumber { get; private set; } = phoneNumber;
        public string? Address { get; private set; } = address;
        public string? CreatedBy { get; private set; } = createdBy;
        public DateTime? CreatedAt { get; private set; } = createdAt;
        public string? UpdatedBy { get; private set; } = updatedBy;
        public DateTime? UpdatedAt { get; private set; } = updatedAt;
        public bool Active { get; private set; } = active;

        public static User Create(Guid externalId, string firstName, string lastName, string email, string? phoneNumber, string? address, string? createdBy, DateTime? createdAt, string? updatedBy, DateTime? updatedAt, bool active)
        {
            return new User(externalId, firstName, lastName, email,phoneNumber, address, createdBy, DateTime.UtcNow, updatedBy, updatedAt, active);
        }
      
    }
}
