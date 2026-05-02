using TeaShop.Application.Auth;
using TeaShop.Application.Products;
using TeaShop.Application.UserManagement;

namespace TeaShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<CategoryService>();

        return services;
    }
}