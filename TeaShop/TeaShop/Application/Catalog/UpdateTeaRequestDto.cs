namespace TeaShop.Application.Catalog;

public sealed record UpdateTeaRequestDto(
    string Name,
    decimal Price,
    int Stock,
    Guid CategoryId
);