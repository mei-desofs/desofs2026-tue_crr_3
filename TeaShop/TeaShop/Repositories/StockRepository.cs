using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class StockRepository(AppDbContext dbContext) : IStockRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<Stock?> GetByProductId(int productId)
        {
            return await _dbContext.Stocks.FirstOrDefaultAsync(s => s.ProductId == productId);
        }

        public async Task<Stock?> GetByExternalId(Guid externalId)
        {
            return await _dbContext.Stocks.FirstOrDefaultAsync(s => s.ExternalId == externalId);
        }

        public async Task Add(Stock stock)
        {
            await _dbContext.Stocks.AddAsync(stock);
            await _dbContext.SaveChangesAsync();
        }

        public async Task Update(Stock stock)
        {
            _dbContext.Stocks.Update(stock);
            await _dbContext.SaveChangesAsync();
        }
    }
}
