using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.Orders;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories;
using Xunit;
using FluentAssertions;

namespace TeaShop.Test.Unit.Infrastructure;

public class OrderRepositoryTests
{
    private TeaShopDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<TeaShopDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new TeaShopDbContext(options);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllOrdersWithItems()
    {
        var context = CreateDbContext();
        var sut = new OrderRepository(context);
        var order = Order.Create(Guid.NewGuid(), [OrderItem.Create(Guid.NewGuid(), 1, 10.0m)]);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await sut.GetAllAsync(CancellationToken.None);

        result.Should().ContainSingle();
        result.First().Items.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrderWhenExists()
    {
        var context = CreateDbContext();
        var sut = new OrderRepository(context);
        var order = Order.Create(Guid.NewGuid(), [OrderItem.Create(Guid.NewGuid(), 1, 10.0m)]);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var result = await sut.GetByIdAsync(order.Id, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Items.Should().HaveCount(1);
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyExistingOrder()
    {
        var context = CreateDbContext();
        var sut = new OrderRepository(context);
        var order = Order.Create(Guid.NewGuid(), [OrderItem.Create(Guid.NewGuid(), 1, 10.0m)]);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        order.UpdateStatus(OrderStatus.Completed);

        await sut.UpdateAsync(order, CancellationToken.None);

        var updatedOrder = await context.Orders.FindAsync(order.Id);
        updatedOrder!.Status.Should().Be(OrderStatus.Completed);
    }
}