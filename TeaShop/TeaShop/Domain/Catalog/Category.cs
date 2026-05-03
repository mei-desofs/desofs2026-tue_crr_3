namespace TeaShop.Domain.Catalog;

using TeaShop.Domain.Exceptions;

public sealed class Category
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    private Category() { }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Category name is required.");

        return new Category
        {
            Id = Guid.NewGuid(),
            Name = name.Trim()
        };
    }
}