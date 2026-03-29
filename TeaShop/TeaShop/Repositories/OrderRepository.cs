using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Enums;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class OrderRepository(AppDbContext dbContext) : IOrderRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

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
