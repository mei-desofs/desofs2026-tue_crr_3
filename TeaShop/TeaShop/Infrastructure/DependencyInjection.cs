using AtleX.HaveIBeenPwned;
using Microsoft.EntityFrameworkCore;
using TeaShop.Domain.IAM;
using TeaShop.Infrastructure.Data;
using TeaShop.Infrastructure.Middleware;
using TeaShop.Infrastructure.Persistence;
using TeaShop.Infrastructure.Persistence.Repositories;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;
using TeaShop.Infrastructure.Security.Interfaces;

namespace TeaShop.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<TeaShopDbContext>(opts =>
            opts.UseNpgsql(config.GetConnectionString("DefaultConnection")));

        services.Configure<ImageStorageSettings>(config.GetSection("ImageStorageSettings"));
        services.AddScoped<IFileUploadService, FileUploadService>();

        services.AddHttpClient<IHaveIBeenPwnedClient, HaveIBeenPwnedClient>(client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("TeaShop-Application/1.0");

            return new HaveIBeenPwnedClient(new HaveIBeenPwnedClientSettings { ApplicationName = "TeaShop" }, client);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
             AllowAutoRedirect = false
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