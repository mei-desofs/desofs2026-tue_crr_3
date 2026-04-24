using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;

namespace TeaShop.Test.Unit.Domain.Users;

public class EmailTests
{
    [Theory]
    [InlineData("user@example.com")]
    [InlineData("USER@EXAMPLE.COM")]        
    [InlineData("  user@example.com  ")]      
    [InlineData("user+tag@sub.example.co.uk")]
    public void Create_ValidEmail_ShouldSucceed(string raw)
    {
        var email = Email.Create(raw);
        email.Value.Should().Be(raw.Trim().ToLowerInvariant());
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_NullOrWhitespace_ShouldThrowDomainException(string? raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<DomainException>().WithMessage("Email is required.");
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain")]
    [InlineData("noatsign")]
    [InlineData("missing@")]
    public void Create_MissingAtSign_ShouldThrowDomainException(string raw)
    {
        var act = () => Email.Create(raw);
        act.Should().Throw<DomainException>().WithMessage("Email format is invalid.");
    }

    [Fact]
    public void Create_ExceedsMaxLength_ShouldThrowDomainException()
    {
        var raw = new string('a', 243) + "@example.com";
        raw.Length.Should().BeGreaterThan(254);

        var act = () => Email.Create(raw);
        act.Should().Throw<DomainException>().WithMessage("Email is too big for it to be valid.");
    }

    [Fact]
    public void Equals_SameValue_ShouldBeTrue()
    {
        var a = Email.Create("user@example.com");
        var b = Email.Create("USER@EXAMPLE.COM");
        a.Should().Be(b);
    }
}