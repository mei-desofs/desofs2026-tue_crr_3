using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Orders;
using Xunit;

namespace TeaShop.Test.Unit.Domain.Orders;

public class OrderTests
{
    private static OrderItem ValidItem() =>
        OrderItem.Create(Guid.NewGuid(), 2, 9.99m);

    // Create

    [Fact]
    public void Create_ValidData_ShouldCreateOrderWithPendingStatus()
    {
        var userId = Guid.NewGuid();

        var order = Order.Create(userId, [ValidItem()]);

        order.Id.Should().NotBeEmpty();
        order.UserId.Should().Be(userId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
        order.Items.Should().HaveCount(1);
    }

    [Fact]
    public void Create_EmptyUserId_ShouldThrowDomainException()
    {
        var act = () => Order.Create(Guid.Empty, [ValidItem()]);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_NoItems_ShouldThrowDomainException()
    {
        var act = () => Order.Create(Guid.NewGuid(), []);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Create_MultipleItems_ShouldIncludeAllItems()
    {
        var items = new[] { ValidItem(), ValidItem(), ValidItem() };

        var order = Order.Create(Guid.NewGuid(), items);

        order.Items.Should().HaveCount(3);
    }

    // Cancel

    [Fact]
    public void Cancel_PendingOrderByOwner_ShouldSetStatusToCancelled()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(userId, [ValidItem()]);

        order.Cancel(userId);

        order.Status.Should().Be(OrderStatus.Cancelled);
    }

    [Fact]
    public void Cancel_ByDifferentUser_ShouldThrowDomainException()
    {
        var order = Order.Create(Guid.NewGuid(), [ValidItem()]);

        var act = () => order.Cancel(Guid.NewGuid());

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Cancel_AlreadyCancelledOrder_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var order = Order.Create(userId, [ValidItem()]);
        order.Cancel(userId);

        var act = () => order.Cancel(userId);

        act.Should().Throw<DomainException>();
    }
}
