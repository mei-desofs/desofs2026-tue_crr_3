using TeaShop.Application.Catalog.DTOs;
using TeaShop.Application.Orders.DTOs;
using TeaShop.Domain.Orders;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Application.Orders;

public sealed class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITeaRepository _teaRepository;

    public OrderService(
        IOrderRepository orderRepository,
        ITeaRepository teaRepository)
    {
        _orderRepository = orderRepository;
        _teaRepository = teaRepository;
    }

    public async Task<OrderDto> CreateAsync(
        Guid userId,
        CreateOrderRequest request,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id");

        if (request.Items.Count == 0)
            throw new ArgumentException("Order must contain items");

        var orderItems = new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var tea = await _teaRepository.GetByIdAsync(item.TeaId, ct);

            if (tea is null)
                throw new KeyNotFoundException("Tea not found");

            if (tea.Stock < item.Quantity)
                throw new ArgumentException($"Insufficient stock for {tea.Name}");

            tea.UpdateStock(tea.Stock - item.Quantity);

            await _teaRepository.UpdateAsync(tea, ct);

            orderItems.Add(OrderItem.Create(
                tea.Id,
                item.Quantity,
                tea.Price
            ));
        }

        var order = Order.Create(userId, orderItems);

        await _orderRepository.AddAsync(order, ct);

        return new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.TeaId,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );
    }
    
    public async Task<List<OrderDto>> GetMyOrdersAsync(
        Guid userId,
        CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id");

        var orders = await _orderRepository.GetByUserIdAsync(userId, ct);

        return orders.Select(order => new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.TeaId,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        )).ToList();
    }

    public async Task<List<OrderDto>> GetAllOrdersAsync(CancellationToken ct)
    {
        var orders = await _orderRepository.GetAllAsync(ct);

        return orders.Select(order => new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.TeaId,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        )).ToList();
    }

    public async Task<OrderDto> UpdateOrderStatusAsync(
        Guid orderId,
        UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        if (!Enum.TryParse<OrderStatus>(request.Status, ignoreCase: true, out var newStatus))
            throw new ArgumentException($"Invalid status '{request.Status}'. Valid values are: Completed, Cancelled.");

        var order = await _orderRepository.GetByIdAsync(orderId, ct);

        if (order is null)
            throw new KeyNotFoundException("Order not found.");

        order.UpdateStatus(newStatus);

        await _orderRepository.UpdateAsync(order, ct);

        return new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.TeaId,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );
    }
    public async Task<OrderDto> CancelAsync(
    Guid userId,
    Guid orderId,
    CancellationToken ct)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("Invalid user id");

        if (orderId == Guid.Empty)
            throw new ArgumentException("Invalid order id");

        var order = await _orderRepository.GetByIdAsync(orderId, ct);

        if (order is null)
            throw new KeyNotFoundException("Order not found");

        order.Cancel(userId);

        await _orderRepository.UpdateAsync(order, ct);

        return new OrderDto(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.CreatedAt,
            order.Items.Select(i => new OrderItemDto(
                i.TeaId,
                i.Quantity,
                i.UnitPrice
            )).ToList()
        );
    }
}