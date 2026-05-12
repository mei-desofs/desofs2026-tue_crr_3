using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.IAM;

public sealed record PasswordHash
{
    public string Value { get; }

    public PasswordHash(string hash)
    {
        if (string.IsNullOrWhiteSpace(hash))
            throw new DomainException("Password hash cannot be empty.");

        if (hash.Length < 15)
        {
            throw new DomainException("Password must have 15 characters.");
        }


        Value = hash;
    }
}