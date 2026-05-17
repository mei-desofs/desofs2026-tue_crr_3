using System.ComponentModel.DataAnnotations;

namespace TeaShop.Application.Orders.DTOs;

public sealed record CreateOrderItemRequest(
    Guid TeaId,
    [Range(1, 1000)] int Quantity
);

public sealed record CreateOrderRequest(
    List<CreateOrderItemRequest> Items
);

public sealed record OrderItemDto(
    Guid TeaId,
    int Quantity,
    decimal UnitPrice
);

public sealed record OrderDto(
    Guid Id,
    Guid UserId,
    string Status,
    DateTime CreatedAt,
    List<OrderItemDto> Items
);

public sealed record UpdateOrderStatusRequest(
    [Required] string Status
);