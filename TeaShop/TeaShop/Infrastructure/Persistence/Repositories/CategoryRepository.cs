using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.Products;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Persistence.Repositories;

public sealed class CategoryRepository : ICategoryRepository
{
    private readonly TeaShopDbContext _db;

    public CategoryRepository(TeaShopDbContext db) => _db = db;

    public async Task<Category?> FindByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Categories.FindAsync([id], ct);

    public async Task<bool> ExistsByNameAsync(string name, CancellationToken ct) =>
        await _db.Categories.AnyAsync(c => c.Name.ToLower() == name.ToLower(), ct);

    public async Task AddAsync(Category category, CancellationToken ct) =>
        await _db.Categories.AddAsync(category, ct);

    public Task RemoveAsync(Category category, CancellationToken ct)
    {
        _db.Categories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);
}
