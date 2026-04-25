using Microsoft.AspNetCore.Identity;

namespace TeaShop.Infrastructure.Security;


public sealed class PasswordHashingService
{
    private readonly PasswordHasher<string> _hasher = new();

    public string Hash(string password) =>
        _hasher.HashPassword(string.Empty, password);

    public bool Verify(string storedHash, string providedPassword)
    {
        var result = _hasher.VerifyHashedPassword(string.Empty, storedHash, providedPassword);

        return result != PasswordVerificationResult.Failed;
    }
}