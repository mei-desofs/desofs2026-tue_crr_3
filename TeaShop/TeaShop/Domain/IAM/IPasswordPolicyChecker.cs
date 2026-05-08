public interface IPasswordPolicyChecker
{
    Task<bool> IsValidAsync(string password);
}