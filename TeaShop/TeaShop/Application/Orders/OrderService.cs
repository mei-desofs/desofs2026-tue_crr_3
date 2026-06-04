using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Text;
using TeaShop.Application.Catalog.DTOs;
using TeaShop.Application.Orders.DTOs;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Orders;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Application.Orders;

public sealed class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly ITeaRepository _teaRepository;
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(
        IOrderRepository orderRepository,
        ITeaRepository teaRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _teaRepository = teaRepository;
        _unitOfWork = unitOfWork;
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

        await _unitOfWork.BeginTransactionAsync(ct);
        try
        {
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

            try
            {
                await _unitOfWork.CommitAsync(ct);
            }
            catch (DbUpdateConcurrencyException) 
            {
                throw new DomainException("This item's stock was updated by another transaction");
            }

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
        catch
        {
            await _unitOfWork.RollbackAsync(ct);
            throw;
        }
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

    public async Task<(byte[] Content, string ContentType, string FileName)> ExportSalesReportAsync(
        ExportSalesReportRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.ReportName))
            throw new ArgumentException("Report name is required.");

        if (request.StartDate > request.EndDate)
            throw new ArgumentException("Start date must be earlier than or equal to end date.");

        var orders = await _orderRepository.GetOrdersInDateRangeAsync(request.StartDate, request.EndDate, ct);

        var csvBytes = GenerateCsvBuffer(orders);

        var sanitizedFileName = SanitizeFileName(request.ReportName);
        var finalFileName = $"{sanitizedFileName}_{DateTime.UtcNow:yyyyMMddHHmmss}.csv";

        return (csvBytes, "text/csv", finalFileName);
    }

    private static byte[] GenerateCsvBuffer(List<Order> orders)
    {
        using var ms = new MemoryStream();
        using var writer = new StreamWriter(ms, Encoding.UTF8);

        writer.WriteLine("OrderId,UserId,Status,CreatedAt,TotalAmount");

        foreach (var order in orders)
        {
            var totalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            var escapedStatus = EscapeCsvField(order.Status.ToString());
            var escapedOrderId = EscapeCsvField(order.Id.ToString());
            var escapedUserId = EscapeCsvField(order.UserId.ToString());

            // Format total amount to guarantee . is always used as separator 
            var formattedTotal = totalAmount.ToString("F2", CultureInfo.InvariantCulture);

            writer.WriteLine($"{escapedOrderId},{escapedUserId},{escapedStatus},{order.CreatedAt:O},{formattedTotal}");
        }

        writer.Flush();
        return ms.ToArray();
    }

    private static string SanitizeFileName(string input)
    {
        char[] invalidChars = { '/', '\\', ':', '*', '?', '"', '<', '>', '|', '\0' };

        var sanitized = string.Concat(input.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));


        // dots are counted as valid characters in some contexts, so we need to explicitly remove the .. to stop path traversal attacks
        while (sanitized.Contains(".."))
        {
            sanitized = sanitized.Replace("..", "");
        }

        sanitized = Path.GetFileName(sanitized);

        if (string.IsNullOrWhiteSpace(sanitized))
        {
            return "SalesReport";
        }

        return sanitized;
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        // Stops a name in the system from being interpreted as a formula when opened in Excel 
        char[] formulaTriggers = { '=', '+', '-', '@', '\t', '\r' };
        if (formulaTriggers.Any(field.StartsWith))
        {
            field = "'" + field;
        }

        // Standard escaping for CSV formatting
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r'))
        {
            field = "\"" + field.Replace("\"", "\"\"") + "\"";
        }

        return field;
    }
}