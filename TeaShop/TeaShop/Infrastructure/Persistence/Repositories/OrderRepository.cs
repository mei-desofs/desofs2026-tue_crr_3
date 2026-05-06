using TeaShop.Domain.Orders;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Persistence.Repositories;

using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public sealed class OrderRepository : IOrderRepository
{
    private readonly TeaShopDbContext _context;

    public OrderRepository(TeaShopDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Order order, CancellationToken ct)
    {
        await _context.Orders.AddAsync(order, ct);
        await _context.SaveChangesAsync(ct);
    }
}