namespace TeaShop.Application.Catalog;

public sealed record CreateTeaRequestDto(
    string Name,
    decimal Price,
    int Stock,
    Guid CategoryId
);