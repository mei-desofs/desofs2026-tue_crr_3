using TeaShop.Domain.Orders;

namespace TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct);
    Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct);
    Task<List<Order>> GetAllAsync(CancellationToken ct);
    Task<Order?> GetByIdAsync(Guid orderId, CancellationToken ct);
    Task UpdateAsync(Order order, CancellationToken ct);
}