using TeaShop.Domain.Catalog;

namespace TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public interface ITeaRepository
{
    Task<List<Tea>> GetAllAsync(CancellationToken ct);
    Task<Tea?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Tea tea, CancellationToken ct);
    void Remove(Tea tea);
    Task SaveChangesAsync(CancellationToken ct);
    Task UpdateAsync(Tea tea, CancellationToken ct);

}