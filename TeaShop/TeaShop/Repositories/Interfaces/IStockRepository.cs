using TeaShop.Domains;

namespace TeaShop.Repositories.Interfaces
{
    public interface IStockRepository
    {
        Task<Stock?> GetByProductId(int productId);
        Task<Stock?> GetByExternalId(Guid externalId);
        Task Add(Stock stock);
        Task Update(Stock stock);
    }
}
