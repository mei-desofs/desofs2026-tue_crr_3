using TeaShop.Domains.Users;

namespace TeaShop.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllActive();
    }
}
