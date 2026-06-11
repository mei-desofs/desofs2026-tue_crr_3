using System.Security.Cryptography;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;

namespace TeaShop.Domain.IAM;

public sealed class Session
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string UserRole { get; private set; } = null!;
    public string TokenHash { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private Session() { }

  
    public static (Session Session, string RawToken) Create(Guid userId, string userRole, TimeSpan? lifetime = null)
    {
        if (!Roles.IsValid(userRole))
            throw new DomainException($"'{userRole}' is not a valid role.");

        var duration = lifetime ?? TimeSpan.FromMinutes(30);
        var rawToken = SessionToken.Generate().ToString();

        var session = new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserRole = userRole,
            TokenHash = HashToken(rawToken), // Store the secure hash
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration),
            IsRevoked = false
        };
        return (session, rawToken);
    }

    public bool IsValid() => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public static string HashToken(string rawToken) =>
        Convert.ToHexString(
            SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rawToken)));
    public void Revoke()
    {
        if (IsRevoked)
            throw new DomainException("Session is already revoked.");

        IsRevoked = true;
    }
}