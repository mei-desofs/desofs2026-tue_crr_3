using Microsoft.EntityFrameworkCore;
using TeaShop.Data;
using TeaShop.Infrastructure.Middleware;
using TeaShop.Infrastructure.Repositories;
using TeaShop.Infrastructure.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<TeaShopDbContext>(opts =>
            opts.UseSqlServer(config.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddSingleton<PasswordHashingService>();

        return services;
    }
    public static WebApplication UseInfrastructureMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GenericExceptionMiddleware>();

        app.UseMiddleware<IAMMiddleware>();

        return app;
    }
}