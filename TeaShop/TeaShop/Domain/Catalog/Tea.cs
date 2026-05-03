namespace TeaShop.Domain.Catalog;

using TeaShop.Domain.Exceptions;

public sealed class Tea
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public decimal Price { get; private set; }
    public int Stock { get; private set; }
    public Guid CategoryId { get; private set; }

    private Tea() { }

    public static Tea Create(string name, decimal price, int stock, Guid categoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Tea name is required.");

        if (price <= 0)
            throw new DomainException("Price must be greater than zero.");

        if (stock < 0)
            throw new DomainException("Stock cannot be negative.");

        return new Tea
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Price = price,
            Stock = stock,
            CategoryId = categoryId
        };
    }

    public void UpdateStock(int newStock)
    {
        if (newStock < 0)
            throw new DomainException("Stock cannot be negative.");

        Stock = newStock;
    }
}