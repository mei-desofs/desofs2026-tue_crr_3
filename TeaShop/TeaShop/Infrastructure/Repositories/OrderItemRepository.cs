using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories
{
    public class OrderItemRepository(TeaShopDBContext dbContext) : IOrderItemRepository
    {
        private readonly TeaShopDBContext _dbContext = dbContext;

        public async Task<IEnumerable<OrderItem>> GetByOrderId(int orderId)
        {
            return await _dbContext.OrderItems.Where(oi => oi.OrderId == orderId).ToListAsync();
        }

        public async Task Add(OrderItem orderItem)
        {
            await _dbContext.OrderItems.AddAsync(orderItem);
            await _dbContext.SaveChangesAsync();
        }
    }
}
