using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class PaymentRepository(AppDbContext dbContext) : IPaymentRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

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
