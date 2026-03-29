using TeaShop.Domains;

namespace TeaShop.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllActive();
    }
}
