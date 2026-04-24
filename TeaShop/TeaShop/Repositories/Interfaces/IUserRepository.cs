using TeaShop.Domain.Users;

namespace TeaShop.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllActive();
    }
}
