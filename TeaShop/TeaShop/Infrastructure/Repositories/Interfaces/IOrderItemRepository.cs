using TeaShop.Domain;

namespace TeaShop.Infrastructure.Repositories.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderId(int orderId);
        Task Add(OrderItem orderItem);
    }
}
