using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;

namespace TeaShop.Tests.Unit.Domain;

public class SessionTokenTests
{
    [Fact]
    public void Generate_ShouldProduceNonEmptyToken()
    {
        var token = SessionToken.Generate();
        token.Value.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Generate_TwoTokens_ShouldNeverBeEqual()
    {
        var a = SessionToken.Generate();
        var b = SessionToken.Generate();
        a.Should().NotBe(b);
    }

    [Fact]
    public void Generate_ShouldProduceUrlSafeValue()
    {
        for (var i = 0; i < 50; i++)
        {
            var token = SessionToken.Generate();
            token.Value.Should().NotContain("+").And.NotContain("/").And.NotContain("=");
        }
    }

    [Fact]
    // 43 characters is the 32 minimum size when encoded to base64.
    public void Generate_ShouldProduceAtLeast43Characters()
    {
  
        var token = SessionToken.Generate();
        token.Value.Length.Should().BeGreaterThanOrEqualTo(43);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FromExisting_EmptyValue_ShouldThrowDomainException(string? raw)
    {
        var act = () => SessionToken.FromExisting(raw);
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void Equals_SameValue_ShouldBeTrue()
    {
        var a = SessionToken.FromExisting("abc123");
        var b = SessionToken.FromExisting("abc123");
        a.Should().Be(b);
    }
}