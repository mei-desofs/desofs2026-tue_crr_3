using TeaShop.Domain;

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
