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
    private readonly IPasswordPolicyChecker _policyChecker;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository users,
        ISessionRepository sessions,
        PasswordHashingService hasher,
        IPasswordPolicyChecker policyChecker,
        ILogger<AuthService> logger)
    {
        _users = users;
        _sessions = sessions;
        _hasher = hasher;
        _policyChecker = policyChecker;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest req, CancellationToken ct)
    {
        if (await _users.ExistsByEmailAsync(req.Email, ct))
            throw new ConflictException("Registration could not be completed email already in use.");
       
        if (!await _policyChecker.IsValidAsync(req.Password))
            throw new DomainException("Password does not meet security policies.");

        var user = User.CreateCustomer(req.Email, _hasher.Hash(req.Password));
        await _users.AddAsync(user, ct);

        var (session, rawToken) = Session.Create(user.Id, user.Role);
        await _sessions.AddAsync(session, ct);
        await _users.SaveChangesAsync(ct);

        _logger.LogInformation("User registered. EmailHash: {Hash}",
            HashEmail(req.Email));

        return new AuthResponse(rawToken, session.ExpiresAt, user.Role);
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest req, CancellationToken ct)
    {
        var user = await _users.FindByEmailAsync(req.Email, ct);

        var hashToVerify = user?.PasswordHash?.Value ?? _hasher.Hash("dummy-to-prevent-timing-leak");


        if (user == null)
        {
            _logger.LogWarning("Failed login attempt. EmailHash: {Hash}",
                HashEmail(req.Email));
            throw new UnauthorizedException("Invalid credentials.");
        }

        var passwordValid = _hasher.Verify(hashToVerify, req.Password);

        if ( user.IsLockedOut)
        {
            _logger.LogWarning("Locked out login attempt. EmailHash: {Hash}",
                HashEmail(req.Email));
            throw new UnauthorizedException("Invalid Credentials");
        }

        if (!passwordValid)
        {
            _logger.LogWarning("Failed login attempt. EmailHash: {Hash}",
                HashEmail(req.Email));

            user.RegisterFailedLogin();

            await _users.SaveChangesAsync(ct);

            throw new UnauthorizedException("Invalid credentials.");
        }

        user.ResetLoginAttempts();
        await _users.SaveChangesAsync(ct);

        var (session, rawToken) = Session.Create(user.Id, user.Role);
        await _sessions.AddAsync(session, ct);
        await _sessions.SaveChangesAsync(ct);

        _logger.LogInformation("Successful login. UserId: {UserId}", user.Id);

        return new AuthResponse(rawToken, session.ExpiresAt, user.Role);
    }

    public async Task LogoutAsync(Guid sessionId, CancellationToken ct)
    {
        var session = await _sessions.GetByIdAsync(sessionId, ct)
            ?? throw new NotFoundException("Session not found.");

        session.Revoke();
        await _sessions.SaveChangesAsync(ct);

        _logger.LogInformation("Session revoked. SessionId: {SessionId}", sessionId);
    }

    public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequest req, CancellationToken ct)
    {
        var user = await _users.FindByIdAsync(userId, ct)
            ?? throw new NotFoundException("User not found.");

        var isCurrentPasswordValid = _hasher.Verify(user.PasswordHash.Value, req.CurrentPassword);
        if (!isCurrentPasswordValid)
            throw new UnauthorizedException("Current password is incorrect.");

        var validationResult = await _policyChecker.IsValidAsync(req.NewPassword);
        if (!validationResult)
            throw new DomainException("New password does not meet security policies.");

        var newHash = _hasher.Hash(req.NewPassword);
        user.UpdatePassword(new PasswordHash(newHash));

        await _users.SaveChangesAsync(ct);

        _logger.LogInformation("Password changed for UserId: {UserId}", userId);
    }


    // NOTE: CodeQL may flag this as PII exposure but this is very healpfull for debugging and monitoring while still
    // not exposing directly the actual email address.
    private static string HashEmail(string email) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(email.ToLowerInvariant())));
}