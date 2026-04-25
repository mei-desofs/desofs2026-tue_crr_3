using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories
{
    public class PaymentRepository(TeaShopDBContext dbContext) : IPaymentRepository
    {
        private readonly TeaShopDBContext _dbContext = dbContext;

        public async Task<Payment?> GetByOrderId(int orderId)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByExternalId(Guid externalId)
        {
            return await _dbContext.Payments.FirstOrDefaultAsync(p => p.ExternalId == externalId);
        }

        public async Task Add(Payment payment)
        {
            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Payment payment)
        {
            _dbContext.Payments.Update(payment);
            await _dbContext.SaveChangesAsync();
        }
    }
}
