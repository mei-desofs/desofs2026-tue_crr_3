using System.Linq;
using System.Text.Json;
using TeaShop.Domain.Exceptions;

namespace TeaShop.Infrastructure.Middleware;

public sealed class GenericExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GenericExceptionMiddleware> _logger;

    public GenericExceptionMiddleware(RequestDelegate next,
        ILogger<GenericExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        try
        {
            await _next(ctx);
        }
        catch (Exception ex)
        {
            await HandleAsync(ctx, ex);
        }
    }

    private async Task HandleAsync(HttpContext ctx, Exception ex)
    {
        var sanitizedPath = SanitizeForLog(ctx.Request.Path.ToString());

        _logger.LogError(ex, "Unhandled exception on {Method} {Path}",
            ctx.Request.Method, sanitizedPath);

        var (statusCode, message) = ex switch
        {
            UnauthorizedException => (StatusCodes.Status401Unauthorized, ex.Message),
            ForbiddenException => (StatusCodes.Status403Forbidden, ex.Message),
            NotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            ConflictException => (StatusCodes.Status409Conflict, ex.Message),
            DomainException => (StatusCodes.Status400BadRequest, ex.Message),

            _ => (StatusCodes.Status500InternalServerError,
                  "An unexpected error occurred. Please try again later.")
        };

        ctx.Response.StatusCode = statusCode;
        ctx.Response.ContentType = "application/json";

        var body = JsonSerializer.Serialize(new { error = message });
        await ctx.Response.WriteAsync(body);
    }

    private static string SanitizeForLog(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        return new string(input.Where(c => !char.IsControl(c)).ToArray());
    }
}