namespace TeaShop.Application.Catalog.DTOs;

public sealed record TeaDto(
    Guid Id,
    string Name,
    decimal Price,
    int Stock
);