using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Infrastructure.Security;
using Xunit;

namespace TeaShop.Tests.Unit.Infrastructure;

public class PasswordHashingServiceTests
{
    private readonly PasswordHashingService _sut = new();

    [Fact]
    public void Hash_ShouldReturnNonEmptyString()
    {
        var hash = _sut.Hash("mypassword123");
        hash.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Hash_SamePlaintext_ShouldProduceDifferentHashes()
    {
        var a = _sut.Hash("mypassword123");
        var b = _sut.Hash("mypassword123");
        a.Should().NotBe(b);
    }

    [Fact]
    public void Verify_CorrectPassword_ShouldReturnTrue()
    {
        var hash = _sut.Hash("correctpassword");
        _sut.Verify(hash, "correctpassword").Should().BeTrue();
    }

    [Fact]
    public void Verify_WrongPassword_ShouldReturnFalse()
    {
        var hash = _sut.Hash("correctpassword");
        _sut.Verify(hash, "wrongpassword").Should().BeFalse();
    }

    [Fact]
    public void Verify_EmptyPassword_ShouldReturnFalse()
    {
        var hash = _sut.Hash("correctpassword");
        _sut.Verify(hash, "").Should().BeFalse();
    }
}

