using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.IAM;

public sealed record PasswordHash
{
    public string Value { get; }

    public PasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty.");

        Value = hash;
    }
}