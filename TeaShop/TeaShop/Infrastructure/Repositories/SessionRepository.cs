using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain.IAM;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories;

public sealed class SessionRepository : ISessionRepository
{
    private readonly TeaShopDbContext _db;

    public SessionRepository(TeaShopDbContext db) => _db = db;

    public async Task<Session?> FindByTokenAsync(string tokenValue, CancellationToken ct) =>
        await _db.Sessions
            .FirstOrDefaultAsync(s => s.Token == SessionToken.FromExisting(tokenValue), ct);

    public async Task<Session?> GetByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Sessions.FindAsync([id], ct);

    public async Task AddAsync(Session session, CancellationToken ct) =>
        await _db.Sessions.AddAsync(session, ct);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);
}