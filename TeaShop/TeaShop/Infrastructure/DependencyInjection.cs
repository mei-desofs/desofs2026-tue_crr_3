using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Persistence.Repositories;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;
namespace TeaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        var connectionString = config.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("DefaultConnection is not configured.");

        connectionString = NormalizePostgresConnectionString(connectionString);

        services.AddDbContext<TeaShopDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITeaRepository, TeaRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<PasswordHashingService>();
        return services;
    }

    private static string NormalizePostgresConnectionString(string connectionString)
    {
        if (!connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) &&
            !connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            return connectionString;
        }

        var uri = new Uri(connectionString);

        var userInfo = uri.UserInfo.Split(':', 2);
        var username = Uri.UnescapeDataString(userInfo[0]);
        var password = userInfo.Length > 1
            ? Uri.UnescapeDataString(userInfo[1])
            : string.Empty;

        var database = uri.AbsolutePath.TrimStart('/');

        var portPart = uri.Port > 0
            ? $"Port={uri.Port};"
            : string.Empty;

        return
            $"Host={uri.Host};" +
            portPart +
            $"Database={database};" +
            $"Username={username};" +
            $"Password={password};" +
            $"SSL Mode=Require;" +
            $"Trust Server Certificate=true";
    }
}