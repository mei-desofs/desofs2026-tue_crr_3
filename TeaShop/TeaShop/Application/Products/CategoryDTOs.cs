using System.ComponentModel.DataAnnotations;

namespace TeaShop.Application.Products;

public sealed record CreateCategoryRequest(
    [Required, MaxLength(100)] string Name,
    [MaxLength(500)] string? Description);

public sealed record UpdateCategoryRequest(
    [MaxLength(100)] string? Name,
    [MaxLength(500)] string? Description);

public sealed record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    DateTime CreatedAt);
