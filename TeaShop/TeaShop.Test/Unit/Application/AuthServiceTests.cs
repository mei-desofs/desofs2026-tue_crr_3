using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using TeaShop.Application.Auth;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;
using TeaShop.Infrastructure.Persistence.Repositories.Interfaces;
using TeaShop.Infrastructure.Security;

namespace TeaShop.Tests.Unit.Application;

public class AuthServiceTests
{
    private readonly IUserRepository _users = Substitute.For<IUserRepository>();
    private readonly ISessionRepository _sessions = Substitute.For<ISessionRepository>();
    private readonly PasswordHashingService _hasher = new();
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        _sut = new AuthService(_users, _sessions, _hasher, NullLogger<AuthService>.Instance);
    }


    [Fact]
    public async Task RegisterAsync_NewUser_ShouldReturnTokenAndRole()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), TestContext.Current.CancellationToken).Returns(false);

        var result = await _sut.RegisterAsync(new RegisterRequest("user@test.com", "Str0ng!Pass"), TestContext.Current.CancellationToken);

        result.Token.Should().NotBeNullOrWhiteSpace();
        result.Role.Should().Be(Roles.Customer);
        result.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        await _users.Received(1).AddAsync(Arg.Any<User>(), TestContext.Current.CancellationToken);
        await _sessions.Received(1).AddAsync(Arg.Any<Session>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task RegisterAsync_DuplicateEmail_ShouldThrowConflictException()
    {
        _users.ExistsByEmailAsync(Arg.Any<string>(), TestContext.Current.CancellationToken).Returns(true);

        var act = () => _sut.RegisterAsync(
            new RegisterRequest("user@test.com", "Str0ng!Pass"), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<ConflictException>();
        await _users.DidNotReceive().AddAsync(Arg.Any<User>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ShouldReturnToken()
    {
        var hash = _hasher.Hash("Str0ng!Pass");
        var user = User.Create("user@test.com", hash);

        _users.FindByEmailAsync("user@test.com", TestContext.Current.CancellationToken).Returns(user);

        var result = await _sut.LoginAsync(
            new LoginRequest("user@test.com", "Str0ng!Pass"), TestContext.Current.CancellationToken);

        result.Token.Should().NotBeNullOrWhiteSpace();
        await _sessions.Received(1).AddAsync(Arg.Any<Session>(), TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task LoginAsync_WrongPassword_ShouldThrowUnauthorizedException()
    {
        var user = User.Create("user@test.com", _hasher.Hash("correct"));
        _users.FindByEmailAsync("user@test.com", TestContext.Current.CancellationToken).Returns(user);

        var act = () => _sut.LoginAsync(
            new LoginRequest("user@test.com", "wrong"), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>();
    }

    [Fact]
    public async Task LoginAsync_UnknownEmail_ShouldThrowUnauthorizedException()
    {
        // ASVS V6.3.8: must throw the same exception as wrong password — same error message.
        _users.FindByEmailAsync(Arg.Any<string>(), TestContext.Current.CancellationToken).Returns((User?)null);

        var act = () => _sut.LoginAsync(
            new LoginRequest("ghost@test.com", "anything"), TestContext.Current.CancellationToken);

        await act.Should().ThrowAsync<UnauthorizedException>()
            .WithMessage("Invalid credentials.");
    }


    [Fact]
    public async Task LogoutAsync_ValidSession_ShouldRevokeIt()
    {
        var session = Session.Create(Guid.NewGuid(), Roles.Customer);
        _sessions.GetByIdAsync(session.Id, TestContext.Current.CancellationToken).Returns(session);

        await _sut.LogoutAsync(session.Id, TestContext.Current.CancellationToken);

        session.IsRevoked.Should().BeTrue();
        await _sessions.Received(1).SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task LogoutAsync_UnknownSessionId_ShouldThrowNotFoundException()
    {
        _sessions.GetByIdAsync(Arg.Any<Guid>(), TestContext.Current.CancellationToken).Returns((Session?)null);

        var act = () => _sut.LogoutAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);
        await act.Should().ThrowAsync<NotFoundException>();
    }
}