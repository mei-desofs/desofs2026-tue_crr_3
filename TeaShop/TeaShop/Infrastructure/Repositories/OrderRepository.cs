using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain;
using TeaShop.Enums;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories
{
    public class OrderRepository(TeaShopDBContext dbContext) : IOrderRepository
    {
        private readonly TeaShopDBContext _dbContext = dbContext;

        public async Task<IEnumerable<Order>> GetByUserId(int userId)
        {
            return await _dbContext.Orders.Where(o => o.UserId == userId).ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetByStatus(OrderStatus status)
        {
            return await _dbContext.Orders.Where(o => o.Status == status).ToListAsync();
        }

        public async Task<Order?> GetByExternalId(Guid externalId)
        {
            return await _dbContext.Orders
                .Include(o => o.OrderItems)
                .FirstOrDefaultAsync(o => o.ExternalId == externalId);
        }

        public async Task Add(Order order)
        {
            await _dbContext.Orders.AddAsync(order);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Order order)
        {
            _dbContext.Orders.Update(order);
            await _dbContext.SaveChangesAsync();
        }
    }
}
