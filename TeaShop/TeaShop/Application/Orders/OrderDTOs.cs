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

public sealed record ExportSalesReportRequest(
    string ReportName,
    DateTime StartDate,
    DateTime EndDate) : IValidatableObject
{
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(ReportName))
            yield return new ValidationResult("Report name is required.", [nameof(ReportName)]);

        if (StartDate > EndDate)
            yield return new ValidationResult("Start date must be earlier than or equal to end date.", [nameof(StartDate), nameof(EndDate)]);
    }
}