using TeaShop.Domain.IAM;

namespace TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public interface ISessionRepository
{
    Task<Session?> FindByTokenAsync(string tokenValue, CancellationToken ct);
    Task<Session?> GetByIdAsync(Guid id, CancellationToken ct);
    Task AddAsync(Session session, CancellationToken ct);
    Task SaveChangesAsync(CancellationToken ct);
}