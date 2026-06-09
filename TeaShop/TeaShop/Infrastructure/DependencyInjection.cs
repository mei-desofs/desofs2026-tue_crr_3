using Microsoft.EntityFrameworkCore;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Middleware;
using TeaShop.Infrastructure.Persistence.Repositories;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;
using AtleX.HaveIBeenPwned;
using TeaShop.Domain.IAM;
using TeaShop.Infrastructure.Persistence;

namespace TeaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<TeaShopDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.AddHttpClient<IHaveIBeenPwnedClient, HaveIBeenPwnedClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TeaShop-Application/1.0");

            return new HaveIBeenPwnedClient(new HaveIBeenPwnedClientSettings { ApplicationName = "TeaShop" }, client);
        });
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISessionRepository, SessionRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<ITeaRepository, TeaRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IPasswordPolicyChecker, PasswordPolicyChecker>();

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