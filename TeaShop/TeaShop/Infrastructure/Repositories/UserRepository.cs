using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly TeaShopDbContext _db;

    public UserRepository(TeaShopDbContext db) => _db = db;

    public async Task<User?> FindByIdAsync(Guid id, CancellationToken ct) =>
        await _db.Users.FindAsync([id], ct);

    public async Task<User?> FindByEmailAsync(string email, CancellationToken ct) =>
        await _db.Users.FirstOrDefaultAsync(u => u.Email == Email.Create(email), ct);

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        await _db.Users.AnyAsync(u => u.Email == Email.Create(email), ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await _db.Users.AddAsync(user, ct);

    public async Task SaveChangesAsync(CancellationToken ct) =>
        await _db.SaveChangesAsync(ct);
}