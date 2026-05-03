using TeaShop.Domain.Products;

namespace TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> FindByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct);
    Task AddAsync(Category category, CancellationToken ct);
    Task RemoveAsync(Category category, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}
