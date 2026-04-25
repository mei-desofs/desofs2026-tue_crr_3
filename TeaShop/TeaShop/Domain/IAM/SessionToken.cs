using System.Security.Cryptography;
using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.IAM;

public sealed class SessionToken
{
    public string Value { get; }

    private SessionToken(string value) => Value = value;

    public static SessionToken Generate()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return new SessionToken(Base64UrlEncode(bytes));
    }

    public static SessionToken FromExisting(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("Token value cannot be empty.");

        return new SessionToken(raw);
    }

    private static string Base64UrlEncode(byte[] bytes) =>
        Convert.ToBase64String(bytes)
               .TrimEnd('=')
               .Replace('+', '-')
               .Replace('/', '_');

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is SessionToken t && t.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}