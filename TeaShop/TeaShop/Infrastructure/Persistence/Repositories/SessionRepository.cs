using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.IAM;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Persistence.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly TeaShopDbContext _db;

    public SessionRepository(TeaShopDbContext db) => _db = db;

    public async Task<Session?> FindByTokenAsync(string rawToken, CancellationToken ct)
    {
        var tokenHash = Session.HashToken(rawToken);

        return await _db.Sessions
            .FirstOrDefaultAsync(s => s.TokenHash == tokenHash, ct);
    }

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Sessions.FindAsync([id], ct);

    public async Task AddAsync(Session session, CancellationToken ct) =>
        await _db.Sessions.AddAsync(session, ct);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);
}