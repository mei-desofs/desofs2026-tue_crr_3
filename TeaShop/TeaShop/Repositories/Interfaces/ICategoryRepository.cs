using TeaShop.Domains;

namespace TeaShop.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetAllActive();
        Task<Category?> GetByExternalId(Guid externalId);
        Task Add(Category category);
        Task Update(Category category);
        Task Deactivate(Guid externalId);
    }
}
