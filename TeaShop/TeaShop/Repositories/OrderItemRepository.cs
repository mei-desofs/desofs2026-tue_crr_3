using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class OrderItemRepository(AppDbContext dbContext) : IOrderItemRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

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
