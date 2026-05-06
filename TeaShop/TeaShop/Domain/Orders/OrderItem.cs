namespace TeaShop.Domain.Orders;

using TeaShop.Domain.Exceptions;

public sealed class OrderItem
{
    public Guid Id { get; private set; }
    public Guid TeaId { get; private set; }
    public int Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    private OrderItem() { }

    public static OrderItem Create(Guid teaId, int quantity, decimal unitPrice)
    {
        if (teaId == Guid.Empty)
            throw new DomainException("Tea id is required.");

        if (quantity <= 0)
            throw new DomainException("Quantity must be greater than zero.");

        if (unitPrice <= 0)
            throw new DomainException("Unit price must be greater than zero.");

        return new OrderItem
        {
            Id = Guid.NewGuid(),
            TeaId = teaId,
            Quantity = quantity,
            UnitPrice = unitPrice
        };
    }
}