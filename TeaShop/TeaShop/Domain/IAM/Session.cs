using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;

namespace TeaShop.Domain.IAM;

public sealed class Session
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string UserRole { get; private set; } = null!;
    public SessionToken Token { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public bool IsRevoked { get; private set; }

    private Session() { }

  
    public static Session Create(Guid userId, string userRole, TimeSpan? lifetime = null)
    {
        if (!Roles.IsValid(userRole))
            throw new DomainException($"'{userRole}' is not a valid role.");

        var duration = lifetime ?? TimeSpan.FromMinutes(30);

        return new Session
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            UserRole = userRole,
            Token = SessionToken.Generate(),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(duration),
            IsRevoked = false
        };
    }

    public bool IsValid() => !IsRevoked && DateTime.UtcNow < ExpiresAt;

    public void Revoke()
    {
        if (IsRevoked)
            throw new DomainException("Session is already revoked.");

        IsRevoked = true;
    }
}