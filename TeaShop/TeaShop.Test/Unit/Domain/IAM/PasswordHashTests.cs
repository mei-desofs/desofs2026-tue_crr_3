using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using Xunit;

namespace TeaShop.Tests.Unit.Domain;

public class PasswordHashTests
{
    [Theory]
    [InlineData("ValidHash_15Chars")] 
    public void Constructor_ValidHash_ShouldCreateInstance(string validHash)
    {
        var passwordHash = new PasswordHash(validHash);

        passwordHash.Value.Should().Be(validHash);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Constructor_EmptyOrWhiteSpace_ShouldThrowDomainException(string invalidHash)
    {
        var act = () => new PasswordHash(invalidHash!);

        act.Should().Throw<DomainException>()
           .WithMessage("Password hash cannot be empty.");
    }

    [Fact]
    public void Constructor_TooShort_ShouldThrowDomainException()
    {
        var shortHash = "ShortHash_1234";

        var act = () => new PasswordHash(shortHash);

        act.Should().Throw<DomainException>()
           .WithMessage("Password must have 15 characters.");
    }

}