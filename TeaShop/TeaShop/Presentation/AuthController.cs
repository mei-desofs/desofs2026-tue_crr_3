using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TeaShop.Application.Auth;
using TeaShop.Infrastructure.RateLimiting;

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
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        if (HttpContext.Items["SessionId"] is not Guid sessionId)
            return Unauthorized();

        await _authService.LogoutAsync(sessionId, ct);
        return NoContent();
    }
}