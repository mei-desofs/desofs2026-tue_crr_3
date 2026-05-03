using TeaShop.Application.Auth;
using TeaShop.Application.Products;
using TeaShop.Application.UserManagement;
using TeaShop.Application.Catalog;

namespace TeaShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<CatalogService>();
        return services;
    }
}