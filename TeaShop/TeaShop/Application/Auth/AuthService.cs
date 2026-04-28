using System.Security.Cryptography;
using System.Text;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Application.Auth;

public sealed class AuthService
{
    private readonly IUserRepository _users;
    private readonly ISessionRepository _sessions;
    private readonly PasswordHashingService _hasher;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        ISessionRepository sessions,
        PasswordHashingService hasher,
        ILogger<AuthService> logger)
    {
        _users = users;
        _sessions = sessions;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(req.Email, ct))
            throw new ConflictException("Registration could not be completed email already in use.");

  

        var user = User.CreateCustomer(req.Email, _hasher.Hash(req.Password));
        await _users.AddAsync(user, ct);

        var session = Session.Create(user.Id, user.Role);
        await _sessions.AddAsync(session, ct);
        await _users.SaveChangesAsync(ct);

        _logger.LogInformation("User registered. EmailHash: {Hash}",
            HashEmail(req.Email));

        return new AuthResponse(session.Token.Value, session.ExpiresAt, user.Role);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(req.Email, ct);

        var hashToVerify = user?.PasswordHash ?? _hasher.Hash("dummy-to-prevent-timing-leak");
        var passwordValid = _hasher.Verify(hashToVerify, req.Password);

        if (user is null || !passwordValid)
        {
            _logger.LogWarning("Failed login attempt. EmailHash: {Hash}",
                HashEmail(req.Email));

            throw new UnauthorizedException("Invalid credentials.");
        }

        var session = Session.Create(user.Id, user.Role);
        await _sessions.AddAsync(session, ct);
        await _sessions.SaveChangesAsync(ct);

        _logger.LogInformation("Successful login. UserId: {UserId}", user.Id);

        return new AuthResponse(session.Token.Value, session.ExpiresAt, user.Role);
    }

    public async Task LogoutAsync(Guid sessionId, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Session not found.");

        session.Revoke();
        await _sessions.SaveChangesAsync(ct);

        _logger.LogInformation("Session revoked. SessionId: {SessionId}", sessionId);
    }


    // NOTE: CodeQL may flag this as PII exposure but this is very healpfull for debugging and monitoring while still
    // not exposing directly the actual email address.
    private static string HashEmail(string email) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant())));
}