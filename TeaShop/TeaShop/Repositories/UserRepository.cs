using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Domains;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Repositories
{
    public class UserRepository(AppDbContext dbContext) : IUserRepository
    {
        private readonly AppDbContext _dbContext = dbContext;

        public async Task<IEnumerable<User>> GetAllActive()
        {
            return await _dbContext.Users.Where(e => e.Active).ToListAsync();
        }
    }
}
