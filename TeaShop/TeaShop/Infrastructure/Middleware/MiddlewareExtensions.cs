using Microsoft.AspNetCore.Builder;

namespace TeaShop.Infrastructure.Middleware;

public static class MiddlewareExtensions
{
    public static WebApplication UseInfrastructureMiddleware(this WebApplication app)
    {
        app.UseMiddleware<GenericExceptionMiddleware>();
        app.UseMiddleware<IAMMiddleware>();

        return app;
    }
}