namespace TeaShop.Application.Catalog;

using Microsoft.AspNetCore.Http;
public sealed record TeaDto(
    Guid Id,
    string Name,
    decimal Price,
    int Stock
);

public sealed record UploadTeaImageDto(
       IFormFile File
       
   );

public sealed record AdjustStockRequest
{
    public required int QuantityChange { get; init; }
}