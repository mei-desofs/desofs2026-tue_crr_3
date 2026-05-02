using System.Security.Claims;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Products;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Application.Products;

public sealed class CategoryService
{
    private readonly ICategoryRepository _categories;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categories, ILogger<CategoryService> logger)
    {
        _categories = categories;
        _logger = logger;
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest req, ClaimsPrincipal caller, CancellationToken ct)
    {
        EnsureAdmin(caller);

        try
        {
            if (await _categories.ExistsByNameAsync(req.Name.Trim(), ct))
                throw new ConflictException($"A category named '{req.Name}' already exists.");

            var category = Category.Create(req.Name, req.Description);

            await _categories.AddAsync(category, ct);
            await _categories.SaveChangesAsync(ct);

            _logger.LogInformation("Category created. Id: {CategoryId}, Name: {Name}", category.Id, category.Name);

            return ToResponse(category);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while creating category.");
            throw;
        }
    }

    public async Task<CategoryResponse> UpdateAsync(Guid id, UpdateCategoryRequest req, ClaimsPrincipal caller, CancellationToken ct)
    {
        EnsureAdmin(caller);

        try
        {
            var category = await _categories.FindByIdAsync(id, ct)
                ?? throw new NotFoundException($"Category '{id}' not found.");

            if (req.Name is not null && !req.Name.Trim().Equals(category.Name, StringComparison.OrdinalIgnoreCase))
            {
                if (await _categories.ExistsByNameAsync(req.Name.Trim(), ct))
                    throw new ConflictException($"A category named '{req.Name}' already exists.");
            }

            category.Update(req.Name, req.Description);

            await _categories.SaveChangesAsync(ct);

            _logger.LogInformation("Category updated. Id: {CategoryId}", category.Id);

            return ToResponse(category);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while updating category. Id: {CategoryId}", id);
            throw;
        }
    }

    public async Task DeleteAsync(Guid id, ClaimsPrincipal caller, CancellationToken ct)
    {
        EnsureAdmin(caller);

        try
        {
            var category = await _categories.FindByIdAsync(id, ct)
                ?? throw new NotFoundException($"Category '{id}' not found.");

            await _categories.RemoveAsync(category, ct);
            await _categories.SaveChangesAsync(ct);

            _logger.LogInformation("Category deleted. Id: {CategoryId}", id);
        }
        catch (Exception ex) when (ex is not DomainException)
        {
            _logger.LogError(ex, "Unexpected error while deleting category. Id: {CategoryId}", id);
            throw;
        }
    }

    private static void EnsureAdmin(ClaimsPrincipal caller)
    {
        if (caller.Identity?.IsAuthenticated != true)
            throw new UnauthorizedException();

        if (!caller.IsInRole(Roles.Admin))
            throw new ForbiddenException();
    }

    private static CategoryResponse ToResponse(Category c) =>
        new(c.Id, c.Name, c.Description, c.CreatedAt);
}
