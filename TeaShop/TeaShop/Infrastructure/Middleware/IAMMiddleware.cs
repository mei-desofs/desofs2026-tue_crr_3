using System.Security.Claims;
using TeaShop.Domain.IAM;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;

namespace TeaShop.Infrastructure.Middleware;

public sealed class IAMMiddleware
{
    private readonly RequestDelegate _next;

    public IAMMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(
        HttpContext ctx,
        ISessionRepository sessions,
        ILogger<IAMMiddleware> logger)
    {
        var tokenValue = ExtractBearerToken(ctx);

        if (tokenValue is not null)
        {
            var session = await sessions.FindByTokenAsync(tokenValue, ctx.RequestAborted);

            if (session is not null && session.IsValid())
            {
                ctx.User = BuildPrincipal(session);
                ctx.Items["SessionId"] = session.Id;
            }
            else if (session is not null)
            {
       
                logger.LogWarning(
                    "Rejected invalid session. Token prefix: {Prefix}, Revoked: {Revoked}",
                    tokenValue[..Math.Min(8, tokenValue.Length)],
                    session.IsRevoked);
            }
        }

        await _next(ctx);
    }

    private static string? ExtractBearerToken(HttpContext ctx)
    {
        var header = ctx.Request.Headers.Authorization.ToString();

        return header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? header["Bearer ".Length..].Trim()
            : null;
    }

    private static ClaimsPrincipal BuildPrincipal(Session session)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Role, session.UserRole),
        };

        var identity = new ClaimsIdentity(claims, authenticationType: "TeaShopSession");
        return new ClaimsPrincipal(identity);
    }
}