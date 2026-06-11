using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaShop.Application.Auth;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;
using Xunit;

namespace TeaShop.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ISessionRepository _sessions = Substitute.For<ISessionRepository>();
    private readonly IPasswordPolicyChecker _policyChecker = Substitute.For<IPasswordPolicyChecker>();
    private readonly PasswordHashingService _hasher = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, _sessions, _hasher, _policyChecker, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task RegisterAsync_NewUser_ShouldReturnTokenAndRole()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), CancellationToken.None).Returns(false);
        _policyChecker.IsValidAsync(Arg.Any<string>()).Returns(true);

        var result = await _sut.RegisterAsync(
            new RegisterRequest("user@test.com", "AJIU54HashedUnlocker"),
            CancellationToken.None);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be(Roles.Customer);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);

        await _users.Received(1).AddAsync(Arg.Any<User>(), CancellationToken.None);
        await _sessions.Received(1).AddAsync(Arg.Any<Session>(), CancellationToken.None);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldThrowConflictException()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), CancellationToken.None).Returns(true);

        var act = () => _sut.RegisterAsync(
            new RegisterRequest("user@test.com", "Str0ng!Pass"),
            CancellationToken.None);

        await act.Should().ThrowAsync<ConflictException>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), CancellationToken.None);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnToken()
    {
        var hash = _hasher.Hash("Str0ng!Pass");
        var user = User.CreateCustomer("user@test.com", hash);

        _users.FindByEmailAsync("user@test.com", CancellationToken.None).Returns(user);

        var result = await _sut.LoginAsync(
            new LoginRequest("user@test.com", "Str0ng!Pass"),
            CancellationToken.None);

        result.Token.Should().NotBeNullOrWhiteSpace();
        await _sessions.Received(1).AddAsync(Arg.Any<Session>(), CancellationToken.None);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldThrowUnauthorizedException()
    {
        var user = User.CreateCustomer("user@test.com", _hasher.Hash("correct"));
        _users.FindByEmailAsync("user@test.com", CancellationToken.None).Returns(user);

        var act = () => _sut.LoginAsync(
            new LoginRequest("user@test.com", "wrong"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ShouldThrowUnauthorizedException()
    {
        _users.FindByEmailAsync(Arg.Any<string>(), CancellationToken.None).Returns((User?)null);

        var act = () => _sut.LoginAsync(
            new LoginRequest("ghost@test.com", "anything"),
            CancellationToken.None);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }

    [Fact]
    public async Task LogoutAsync_ValidSession_ShouldRevokeIt()
    {
        var (session, rawToken) = Session.Create(Guid.NewGuid(), Roles.Customer);
        _sessions.GetByIdAsync(session.Id, CancellationToken.None).Returns(session);

        await _sut.LogoutAsync(session.Id, CancellationToken.None);

        session.IsRevoked.Should().BeTrue();
        await _sessions.Received(1).SaveChangesAsync(CancellationToken.None);
    }

    [Fact]
    public async Task LogoutAsync_UnknownSessionId_ShouldThrowNotFoundException()
    {
        _sessions.GetByIdAsync(Arg.Any<Guid>(), CancellationToken.None).Returns((Session?)null);

        var act = () => _sut.LogoutAsync(Guid.NewGuid(), CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}