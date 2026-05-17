using System.Threading.RateLimiting;

using Microsoft.AspNetCore.RateLimiting;

namespace TeaShop.Infrastructure.Security;

public static class RateLimiting
{
    public const string AuthPolicy = "auth";
    public const string GeneralPolicy = "general";

    public static IServiceCollection AddTeaShopRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(opts =>
        {
            opts.AddPolicy(AuthPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 5,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            opts.AddPolicy(GeneralPolicy, context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 60,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 10
                    }));

            opts.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
        });

        return services;
    }
}