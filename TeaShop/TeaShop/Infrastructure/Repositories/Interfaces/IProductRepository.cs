using TeaShop.Domain;

namespace TeaShop.Infrastructure.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllActive();
        Task<IEnumerable<Product>> GetByCategoryId(int categoryId);
        Task<Product?> GetByExternalId(Guid externalId);
        Task Add(Product product);
        Task Update(Product product);
        Task Deactivate(Guid externalId);
    }
}
