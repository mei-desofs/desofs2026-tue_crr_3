namespace TeaShop.Domain.Orders;

using TeaShop.Domain.Exceptions;

public sealed class Order
{
    private List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public OrderStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    private Order() { }

    public static Order Create(Guid userId, IEnumerable<OrderItem> items)
    {
        if (userId == Guid.Empty)
            throw new DomainException("User id is required.");

        var itemList = items.ToList();

        if (!itemList.Any())
            throw new DomainException("Order must have at least one item.");

        return new Order
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Status = OrderStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            _items = itemList
        };
    }

    public void Cancel(Guid userId)
    {
        if (UserId != userId)
            throw new DomainException("Cannot cancel another user's order.");

        if (Status != OrderStatus.Pending)
            throw new DomainException("Only pending orders can be cancelled.");

        Status = OrderStatus.Cancelled;
    }
}