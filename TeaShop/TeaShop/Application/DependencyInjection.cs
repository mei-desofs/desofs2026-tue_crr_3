using TeaShop.Application.Auth;
using TeaShop.Application.Products;
using TeaShop.Application.UserManagement;
using TeaShop.Application.Catalog;
using TeaShop.Application.Orders;

namespace TeaShop.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<AuthService>();
        services.AddScoped<UserService>();
        services.AddScoped<CategoryService>();
        services.AddScoped<CatalogService>();
        services.AddScoped<OrderService>();
        return services;
    }
}