using TeaShop.Application.Auth;
using TeaShop.Application.UserManagement;

namespace TeaShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();

        return services;
    }
}