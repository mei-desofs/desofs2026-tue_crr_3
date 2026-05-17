using FluentAssertions;
using NSubstitute;
using TeaShop.Application.Orders;
using TeaShop.Application.Orders.DTOs;
using TeaShop.Domain.Catalog;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Orders;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using Xunit;

namespace TeaShop.Test.Unit.Application;

public class OrderServiceTests
{
    private readonly IOrderRepository _orderRepository = Substitute.For<IOrderRepository>();
    private readonly ITeaRepository _teaRepository = Substitute.For<ITeaRepository>();
    private readonly IUnitOfWork _unitOfWork = Substitute.For<IUnitOfWork>();
    private readonly OrderService _sut;

    public OrderServiceTests()
    {
        _sut = new OrderService(_orderRepository, _teaRepository, _unitOfWork);
    }

    private static Tea ValidTea(int stock = 10) =>
        Tea.Create("Green Tea", 9.99m, stock, Guid.NewGuid());

    // CreateAsync

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldReturnOrderDto()
    {
        var userId = Guid.NewGuid();
        var tea = ValidTea();
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 2)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        var result = await _sut.CreateAsync(userId, request, CancellationToken.None);

        result.UserId.Should().Be(userId);
        result.Status.Should().Be("Pending");
        result.Items.Should().HaveCount(1);
        result.Items[0].TeaId.Should().Be(tea.Id);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(tea.Price);
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldPersistOrder()
    {
        var tea = ValidTea();
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 1)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await _orderRepository.Received(1).AddAsync(Arg.Any<Order>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldDecrementTeaStock()
    {
        var tea = ValidTea(stock: 10);
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 3)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        tea.Stock.Should().Be(7);
        await _teaRepository.Received(1).UpdateAsync(tea, CancellationToken.None);
    }

    [Fact]
    public async Task CreateAsync_MultipleItems_ShouldDecrementEachTeaStock()
    {
        var tea1 = ValidTea(stock: 5);
        var tea2 = ValidTea(stock: 8);
        var request = new CreateOrderRequest([
            new CreateOrderItemRequest(tea1.Id, 2),
            new CreateOrderItemRequest(tea2.Id, 3)
        ]);
        _teaRepository.GetByIdAsync(tea1.Id, CancellationToken.None).Returns(tea1);
        _teaRepository.GetByIdAsync(tea2.Id, CancellationToken.None).Returns(tea2);

        await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        tea1.Stock.Should().Be(3);
        tea2.Stock.Should().Be(5);
        await _teaRepository.Received(2).UpdateAsync(Arg.Any<Tea>(), CancellationToken.None);
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ShouldBeginAndCommitTransaction()
    {
        var tea = ValidTea();
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 1)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await _unitOfWork.Received(1).BeginTransactionAsync(CancellationToken.None);
        await _unitOfWork.Received(1).CommitAsync(CancellationToken.None);
        await _unitOfWork.DidNotReceive().RollbackAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_EmptyUserId_ShouldThrowArgumentException()
    {
        var request = new CreateOrderRequest([new CreateOrderItemRequest(Guid.NewGuid(), 1)]);

        var act = async () => await _sut.CreateAsync(Guid.Empty, request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task CreateAsync_EmptyItems_ShouldThrowArgumentException()
    {
        var request = new CreateOrderRequest([]);

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_TeaNotFound_ShouldThrowKeyNotFoundException()
    {
        var request = new CreateOrderRequest([new CreateOrderItemRequest(Guid.NewGuid(), 1)]);
        _teaRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Tea?)null);

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_TeaNotFound_ShouldRollbackTransaction()
    {
        var request = new CreateOrderRequest([new CreateOrderItemRequest(Guid.NewGuid(), 1)]);
        _teaRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Tea?)null);

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _unitOfWork.Received(1).RollbackAsync(CancellationToken.None);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_InsufficientStock_ShouldThrowArgumentException()
    {
        var tea = ValidTea(stock: 2);
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 5)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage($"*{tea.Name}*");
        await _orderRepository.DidNotReceive().AddAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_InsufficientStock_ShouldRollbackTransaction()
    {
        var tea = ValidTea(stock: 2);
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 5)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        var act = async () => await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
        await _unitOfWork.Received(1).RollbackAsync(CancellationToken.None);
        await _unitOfWork.DidNotReceive().CommitAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_ExactStock_ShouldSucceed()
    {
        var tea = ValidTea(stock: 3);
        var request = new CreateOrderRequest([new CreateOrderItemRequest(tea.Id, 3)]);
        _teaRepository.GetByIdAsync(tea.Id, CancellationToken.None).Returns(tea);

        var result = await _sut.CreateAsync(Guid.NewGuid(), request, CancellationToken.None);

        result.Should().NotBeNull();
        tea.Stock.Should().Be(0);
    }

    // GetMyOrdersAsync

    [Fact]
    public async Task GetMyOrdersAsync_ValidUser_ShouldReturnOrders()
    {
        var userId = Guid.NewGuid();
        var tea = ValidTea();
        var orders = new List<Order>
        {
            Order.Create(userId, [OrderItem.Create(tea.Id, 1, tea.Price)]),
            Order.Create(userId, [OrderItem.Create(tea.Id, 2, tea.Price)])
        };
        _orderRepository.GetByUserIdAsync(userId, CancellationToken.None).Returns(orders);

        var result = await _sut.GetMyOrdersAsync(userId, CancellationToken.None);

        result.Should().HaveCount(2);
        result.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetMyOrdersAsync_ShouldNotReturnOtherUsersOrders()
    {
        // The repository is queried by userId — verify isolation is enforced at the service level
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var tea = ValidTea();
        var ownOrders = new List<Order>
        {
            Order.Create(userId, [OrderItem.Create(tea.Id, 1, tea.Price)])
        };
        _orderRepository.GetByUserIdAsync(userId, CancellationToken.None).Returns(ownOrders);
        _orderRepository.GetByUserIdAsync(otherUserId, CancellationToken.None).Returns([]);

        var result = await _sut.GetMyOrdersAsync(userId, CancellationToken.None);

        result.Should().AllSatisfy(o => o.UserId.Should().Be(userId));
        await _orderRepository.Received(1).GetByUserIdAsync(userId, CancellationToken.None);
        await _orderRepository.DidNotReceive().GetByUserIdAsync(otherUserId, CancellationToken.None);
    }

    [Fact]
    public async Task GetMyOrdersAsync_NoOrders_ShouldReturnEmptyList()
    {
        var userId = Guid.NewGuid();
        _orderRepository.GetByUserIdAsync(userId, CancellationToken.None).Returns([]);

        var result = await _sut.GetMyOrdersAsync(userId, CancellationToken.None);

        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyOrdersAsync_EmptyUserId_ShouldThrowArgumentException()
    {
        var act = async () => await _sut.GetMyOrdersAsync(Guid.Empty, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>();
    }

    // GetAllOrdersAsync

    [Fact]
    public async Task GetAllOrdersAsync_ShouldReturnAllOrders()
    {
        var userId = Guid.NewGuid();
        var teaId = Guid.NewGuid();
        var item = OrderItem.Create(teaId, 1, 10.0m);
        var orders = new List<Order> { Order.Create(userId, [item]) };

        _orderRepository.GetAllAsync(CancellationToken.None).Returns(orders);

        var result = await _sut.GetAllOrdersAsync(CancellationToken.None);

        result.Should().HaveCount(1);
    }

    // UpdateOrderStatusAsync

    [Fact]
    public async Task UpdateOrderStatusAsync_ValidStatus_ShouldUpdateStatus()
    {
        var userId = Guid.NewGuid();
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(userId, [item]);

        var request = new UpdateOrderStatusRequest("Completed");

        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var result = await _sut.UpdateOrderStatusAsync(order.Id, request, CancellationToken.None);

        result.Status.Should().Be("Completed");
        await _orderRepository.Received(1).UpdateAsync(order, CancellationToken.None);
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_InvalidStatus_ShouldThrowArgumentException()
    {
        var request = new UpdateOrderStatusRequest("NotAValidStatus");

        var act = async () => await _sut.UpdateOrderStatusAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid status*");
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_OrderNotFound_ShouldThrowKeyNotFoundException()
    {
        var request = new UpdateOrderStatusRequest("Completed");
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Order?)null);

        var act = async () => await _sut.UpdateOrderStatusAsync(Guid.NewGuid(), request, CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_AlreadyCompleted_ShouldThrowDomainException()
    {
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(Guid.NewGuid(), [item]);
        order.UpdateStatus(OrderStatus.Completed);
        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var act = async () => await _sut.UpdateOrderStatusAsync(order.Id, new UpdateOrderStatusRequest("Cancelled"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*pending*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task UpdateOrderStatusAsync_SetToPending_ShouldThrowDomainException()
    {
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(Guid.NewGuid(), [item]);
        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var act = async () => await _sut.UpdateOrderStatusAsync(order.Id, new UpdateOrderStatusRequest("Pending"), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*already pending*");
    }

    // CancelAsync

    [Fact]
    public async Task CancelAsync_ValidRequest_ShouldReturnCancelledOrderDto()
    {
        var userId = Guid.NewGuid();
        var teaId = Guid.NewGuid();
        var item = OrderItem.Create(teaId, 2, 9.99m);
        var order = Order.Create(userId, [item]);

        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var result = await _sut.CancelAsync(userId, order.Id, CancellationToken.None);

        result.Id.Should().Be(order.Id);
        result.UserId.Should().Be(userId);
        result.Status.Should().Be("Cancelled");
        result.Items.Should().HaveCount(1);
        result.Items[0].TeaId.Should().Be(teaId);
        result.Items[0].Quantity.Should().Be(2);
    }

    [Fact]
    public async Task CancelAsync_WrongUser_ShouldThrowDomainException()
    {
        var ownerId = Guid.NewGuid();
        var attackerId = Guid.NewGuid();
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(ownerId, [item]);
        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var act = async () => await _sut.CancelAsync(attackerId, order.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*another user*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_AlreadyCancelledOrder_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(userId, [item]);
        order.Cancel(userId);
        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var act = async () => await _sut.CancelAsync(userId, order.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*pending*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_CompletedOrder_ShouldThrowDomainException()
    {
        var userId = Guid.NewGuid();
        var item = OrderItem.Create(Guid.NewGuid(), 1, 9.99m);
        var order = Order.Create(userId, [item]);
        order.UpdateStatus(OrderStatus.Completed);
        _orderRepository.GetByIdAsync(order.Id, CancellationToken.None).Returns(order);

        var act = async () => await _sut.CancelAsync(userId, order.Id, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*pending*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_OrderNotFound_ShouldThrowKeyNotFoundException()
    {
        _orderRepository.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Order?)null);

        var act = async () => await _sut.CancelAsync(Guid.NewGuid(), Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<KeyNotFoundException>();
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_EmptyUserId_ShouldThrowArgumentException()
    {
        var act = async () => await _sut.CancelAsync(Guid.Empty, Guid.NewGuid(), CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid user id*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CancelAsync_EmptyOrderId_ShouldThrowArgumentException()
    {
        var act = async () => await _sut.CancelAsync(Guid.NewGuid(), Guid.Empty, CancellationToken.None);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Invalid order id*");
        await _orderRepository.DidNotReceive().UpdateAsync(Arg.Any<Order>(), Arg.Any<CancellationToken>());
    }
}
