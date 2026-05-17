git using Microsoft.Extensions.Configuration;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Infrastructure.Persistence.Seed;

public sealed class AdminSeeder
{
    private readonly IUserRepository _users;
    private readonly PasswordHashingService _hasher;
    private readonly IConfiguration _config;
    private readonly ILogger<AdminSeeder> _logger;

    public AdminSeeder(
        IUserRepository users,
        PasswordHashingService hasher,
        IConfiguration config,
        ILogger<AdminSeeder> logger)
    {
        _users = users;
        _hasher = hasher;
        _config = config;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct)
    {
        var email = _config["Seed:AdminEmail"];
        var password = _config["Seed:AdminPassword"];

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Admin seed skipped — Seed:AdminEmail or Seed:AdminPassword not configured.");
            return;
        }

        if (await _users.ExistsByEmailAsync(email, ct))
        {
            _logger.LogInformation("Admin seed skipped — admin account already exists.");
            return;
        }

        var admin = User.CreateStaff(
            email,
            _hasher.Hash(password),
            role: Roles.Admin);

        await _users.AddAsync(admin, ct);
        await _users.SaveChangesAsync(ct);

        _logger.LogWarning("Admin account seeded. UserId: {UserId}", admin.Id);
    }
}