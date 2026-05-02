using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.Users;

public sealed class User
{
    public Guid Id { get; private set; }
    public Email Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Role { get; private set; } = null!;
    public Address? ShippingAddress { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User() { }

    public static User CreateCustomer(string rawEmail, string passwordHash)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(FailureMessages.User.PasswordHashRequired);

        return new User
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(rawEmail),
            PasswordHash = passwordHash,
            Role = Roles.Customer
        };
    }

    public static User CreateStaff(string rawEmail, string passwordHash, string role)
    {
        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException(FailureMessages.User.PasswordHashRequired);

        if (!Roles.IsValid(role) || role == Roles.Customer)
            throw new DomainException(FailureMessages.User.InvalidStaffRole(role));

        return new User
        {
            Id = Guid.NewGuid(),
            Email = Email.Create(rawEmail),
            PasswordHash = passwordHash,
            Role = role
        };
    }

    public void UpdateShippingAddress(Address newAddress)
    {
        ShippingAddress = newAddress;
    }

    public void RemoveShippingAddress()
    {
        ShippingAddress = null;
    }

    public void UpdatePassword(string newPasswordHash)
    {
        if (string.IsNullOrWhiteSpace(newPasswordHash))
            throw new DomainException(FailureMessages.User.PasswordHashEmpty);

        PasswordHash = newPasswordHash;
    }
}
