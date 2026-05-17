using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Orders;
using Xunit;

namespace TeaShop.Test.Unit.Domain.Orders;

public class OrderItemTests
{
    [Fact]
    public void Create_ValidData_ShouldCreateOrderItem()
    {
        var teaId = Guid.NewGuid();

        var item = OrderItem.Create(teaId, 3, 12.50m);

        item.Id.Should().NotBeEmpty();
        item.TeaId.Should().Be(teaId);
        item.Quantity.Should().Be(3);
        item.UnitPrice.Should().Be(12.50m);
    }

    [Fact]
    public void Create_EmptyTeaId_ShouldThrowDomainException()
    {
        var act = () => OrderItem.Create(Guid.Empty, 1, 9.99m);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Create_InvalidQuantity_ShouldThrowDomainException(int quantity)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), quantity, 9.99m);

        act.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-0.01)]
    [InlineData(-10)]
    public void Create_InvalidUnitPrice_ShouldThrowDomainException(double price)
    {
        var act = () => OrderItem.Create(Guid.NewGuid(), 1, (decimal)price);

        act.Should().Throw<DomainException>();
    }
}
