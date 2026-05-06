using TeaShop.Domain.Orders;

namespace TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken ct);
    Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct);
}