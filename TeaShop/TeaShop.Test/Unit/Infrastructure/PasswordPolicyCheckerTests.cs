using FluentAssertions;
using NSubstitute;
using AtleX.HaveIBeenPwned;
using TeaShop.Infrastructure.Security;
using Xunit;

namespace TeaShop.Tests.Unit.Infrastructure.Security;

public class PasswordPolicyCheckerTests
{
    private readonly IHaveIBeenPwnedClient _client;
    private readonly PasswordPolicyChecker _checker;

    public PasswordPolicyCheckerTests()
    {
        _client = Substitute.For<IHaveIBeenPwnedClient>();
        _checker = new PasswordPolicyChecker(_client);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task IsValidAsync_ShouldReturnFalse_WhenPasswordIsEmpty(string password)
    {
        var result = await _checker.IsValidAsync(password!);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidAsync_ShouldReturnFalse_WhenTooShort()
    {
        var result = await _checker.IsValidAsync("shortpwd");

        result.Should().BeFalse();
    }

    [Theory]
    [InlineData("TeaShop1234567890")]
    [InlineData("mypassword123TeaShop")]
    [InlineData("PASSWORD123456TeaShop")]
    public async Task IsValidAsync_ShouldReturnFalse_WhenContainsForbiddenWord(string password)
    {
        var result = await _checker.IsValidAsync(password);

        result.Should().BeFalse();
    }

    [Fact]
    public async Task IsValidAsync_ShouldReturnFalse_WhenPasswordIsPwned()
    {
        _client
            .IsPwnedPasswordAsync(Arg.Any<string>())
            .Returns(true);

        var result = await _checker.IsValidAsync("verystrong6789100");

        result.Should().BeFalse();

        await _client.Received(1).IsPwnedPasswordAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task IsValidAsync_ShouldReturnTrue_WhenPasswordIsValid()
    {
        _client .IsPwnedPasswordAsync(Arg.Any<string>()) .Returns(false);

        var result = await _checker.IsValidAsync("verystrong6789100");

        result.Should().BeTrue();
    }
}