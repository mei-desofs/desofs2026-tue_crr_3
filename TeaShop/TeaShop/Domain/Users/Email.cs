
using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.Users;

public sealed class Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Email Create(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            throw new DomainException("Email is required.");

        var normalised = raw.Trim().ToLowerInvariant();

        var atIndex = normalised.IndexOf('@');
        if (atIndex <= 1 || atIndex == normalised.Length - 1)
            throw new DomainException("Email format is invalid.");

        if (normalised.Length > 254)
            throw new DomainException("Email is too big for it to be valid.");

        return new Email(normalised);
    }

    public override string ToString() => Value;
    public override bool Equals(object? obj) => obj is Email e && e.Value == Value;
    public override int GetHashCode() => Value.GetHashCode();
}