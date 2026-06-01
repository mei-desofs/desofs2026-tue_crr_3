using TeaShop.Domain.Orders;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

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

    public async Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<Order>> GetAllAsync(CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct)
    {
        return await _context.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.Id == orderId, ct);
    }

    public async Task UpdateAsync(Order order, CancellationToken ct)
    {
        _context.Orders.Update(order);
        await _context.SaveChangesAsync(ct);
    }

    public async Task<List<Order>> GetOrdersInDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken ct)
    {
        return await _context.Orders
            .AsNoTracking() // as it's a Read-only query we can disable tracking for better performance
            .Include(o => o.Items) 
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);
    }

}
