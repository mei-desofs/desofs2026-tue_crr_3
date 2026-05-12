using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;
using TeaShop.Application.Auth;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Presentation.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService) => _authService = authService;

    [HttpPost("signUp")]
    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    public async Task<IActionResult> Register(RegisterRequest req, CancellationToken ct)
    {
        var result = await _authService.RegisterAsync(req, ct);
        return Ok(result);
    }

    [HttpPost("login")]
    [EnableRateLimiting(RateLimiting.AuthPolicy)]
    public async Task<IActionResult> Login(LoginRequest req, CancellationToken ct)
    {
        var result = await _authService.LoginAsync(req, ct);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    [EnableRateLimiting(RateLimiting.GeneralPolicy)]

    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (HttpContext.Items["SessionId"] is not Guid sessionId)
            return Unauthorized();

        await _authService.LogoutAsync(sessionId, ct);
        return NoContent();
    }

    [HttpPost("change-password")]
    [Authorize]
    [EnableRateLimiting(RateLimiting.AuthPolicy)]

    public async Task<IActionResult> ChangePassword(ChangePasswordRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? throw new UnauthorizedException("User not identified.");

        await _authService.ChangePasswordAsync(Guid.Parse(userId), req, ct);
        return NoContent(); 
    }
}