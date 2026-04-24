using TeaShop.Domain;

namespace TeaShop.Repositories.Interfaces
{
    public interface IOrderItemRepository
    {
        Task<IEnumerable<OrderItem>> GetByOrderId(int orderId);
        Task Add(OrderItem orderItem);
    }
}
