using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class ProductRepository(AppDbContext dbContext) : IProductRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<IEnumerable<Product>> GetAllActive()
        {
            return await _dbContext.Products.Where(p => p.Active).ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetByCategoryId(int categoryId)
        {
            return await _dbContext.Products.Where(p => p.CategoryId == categoryId && p.Active).ToListAsync();
        }

        public async Task<Product?> GetByExternalId(Guid externalId)
        {
            return await _dbContext.Products.FirstOrDefaultAsync(p => p.ExternalId == externalId);
        }

        public async Task Add(Product product)
        {
            await _dbContext.Products.AddAsync(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Product product)
        {
            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Deactivate(Guid externalId)
        {
            var product = await _dbContext.Products.FirstOrDefaultAsync(p => p.ExternalId == externalId);
            if (product is null) return;

            _dbContext.Products.Update(product);
            await _dbContext.SaveChangesAsync();
        }
    }
}
