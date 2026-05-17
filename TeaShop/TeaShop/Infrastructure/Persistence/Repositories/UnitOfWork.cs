using Microsoft.EntityFrameworkCore.Storage;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Persistence.Repositories;

public sealed class UnitOfWork : IUnitOfWork
{
    private readonly TeaShopDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(TeaShopDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken ct)
    {
        _transaction = await _context.Database.BeginTransactionAsync(ct);
    }

    public async Task CommitAsync(CancellationToken ct)
    {
        await _transaction!.CommitAsync(ct);
        _transaction = null;
    }

    public async Task RollbackAsync(CancellationToken ct)
    {
        if (_transaction is not null)
        {
            await _transaction.RollbackAsync(ct);
            _transaction = null;
        }
    }
}
