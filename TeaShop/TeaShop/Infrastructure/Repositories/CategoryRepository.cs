using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domain;
using TeaShop.Infrastructure.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Repositories
{
    public class CategoryRepository(TeaShopDBContext dbContext) : ICategoryRepository
    {
        private readonly TeaShopDBContext _dbContext = dbContext;

        public async Task<IEnumerable<Category>> GetAllActive()
        {
            return await _dbContext.Categories.Where(c => c.Active).ToListAsync();
        }

        public async Task<Category?> GetByExternalId(Guid externalId)
        {
            return await _dbContext.Categories.FirstOrDefaultAsync(c => c.ExternalId == externalId);
        }

        public async Task Add(Category category)
        {
            await _dbContext.Categories.AddAsync(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Category category)
        {
            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Deactivate(Guid externalId)
        {
            var category = await _dbContext.Categories.FirstOrDefaultAsync(c => c.ExternalId == externalId);
            if (category is null) return;

            _dbContext.Categories.Update(category);
            await _dbContext.SaveChangesAsync();
        }
    }
}
