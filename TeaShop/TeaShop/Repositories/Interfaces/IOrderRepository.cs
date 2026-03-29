using TeaShop.Domains;
using TeaShop.Enums;

namespace TeaShop.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetByUserId(int userId);
        Task<IEnumerable<Order>> GetByStatus(OrderStatus status);
        Task<Order?> GetByExternalId(Guid externalId);
        Task Add(Order order);
        Task Update(Order order);
    }
}
