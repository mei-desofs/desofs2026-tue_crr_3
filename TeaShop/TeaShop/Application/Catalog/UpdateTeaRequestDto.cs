using System.Text.Json.Serialization;

namespace TeaShop.Application.Catalog;

public sealed record UpdateTeaRequestDto(
    string Name,
    [property: JsonRequired] decimal Price,
    [property: JsonRequired] int Stock,
    [property: JsonRequired] Guid CategoryId
);