
namespace TeaShop.Domain.IAM;

public interface IPasswordPolicyChecker
{
    Task<bool> IsValidAsync(string password);
}