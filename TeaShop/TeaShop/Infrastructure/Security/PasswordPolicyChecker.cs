using AtleX.HaveIBeenPwned;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;

namespace TeaShop.Infrastructure.Security;

public sealed class PasswordPolicyChecker : IPasswordPolicyChecker
{
    private readonly List<string> _forbiddenWords = new() { "TeaShop", "Password123" };
    private readonly IHaveIBeenPwnedClient _client;

    public PasswordPolicyChecker(IHaveIBeenPwnedClient client)
    {
        _client = client;
    }

    public async Task<bool> IsValidAsync(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) return false;

        if (password.Length < 15) return false;
        
        if (_forbiddenWords.Any(word => password.Contains(word, StringComparison.OrdinalIgnoreCase)))
            return false;

       
       if (await _client.IsPwnedPasswordAsync(password))
            return false;

        return true;
    }
}