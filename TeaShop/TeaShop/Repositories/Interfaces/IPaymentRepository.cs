using TeaShop.Domains;

namespace TeaShop.Repositories.Interfaces
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderId(int orderId);
        Task<Payment?> GetByExternalId(Guid externalId);
        Task Add(Payment payment);
        Task Update(Payment payment);
    }
}
