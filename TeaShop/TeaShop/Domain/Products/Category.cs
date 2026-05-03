using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.Products;

public sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Category() { }

    public static Category Create(string name, string? description = null)
    {
        return new Category
        {
            Id = Guid.NewGuid(),
            Name = ValidateName(name),
            Description = ValidateDescription(description),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string? name, string? description)
    {
        if (name is not null)
            Name = ValidateName(name);

        if (description is not null)
            Description = ValidateDescription(description);
    }

    private static string ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException(FailureMessages.Category.NameRequired);

        if (name.Length > 100)
            throw new DomainException(FailureMessages.Category.NameTooLong);

        return name.Trim();
    }

    private static string? ValidateDescription(string? description)
    {
        if (description is null)
            return null;

        if (description.Length > 500)
            throw new DomainException(FailureMessages.Category.DescriptionTooLong);

        return description.Trim();
    }
}
