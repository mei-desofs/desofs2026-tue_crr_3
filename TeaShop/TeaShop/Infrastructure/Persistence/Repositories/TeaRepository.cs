using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.Catalog;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Persistence.Repositories;

public sealed class TeaRepository : ITeaRepository
{
    private readonly TeaShopDbContext _context;

    public TeaRepository(TeaShopDbContext context)
    {
        _context = context;
    }

    public async Task<List<Tea>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Teas.ToListAsync(ct);
    }

    public async Task<Tea?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        return await _context.Teas.FindAsync(new object[] { id }, ct);
    }

    public async Task UpdateAsync(Tea tea, CancellationToken ct)
{
    _context.Teas.Update(tea);
    await _context.SaveChangesAsync(ct);
}
}