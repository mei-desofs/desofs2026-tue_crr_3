// tests/TeaShop.Tests.Unit/Domain/UserTests.cs
using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;

namespace TeaShop.Tests.Unit.Domain.Users;

public class UserTests
{
    private const string ValidEmail = "user@example.com";
    private const string ValidHash = "$2a$10$validhashvalue";


    [Fact]
    public void Create_ValidInputs_ShouldSucceed()
    {
        var user = User.Create(ValidEmail, ValidHash);

        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be(ValidEmail);
        user.Role.Should().Be(Roles.Customer);
        user.ShippingAddress.Should().BeNull();
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(2));
    }

    [Fact]
    public void Create_WithExplicitRole_ShouldSetRole()
    {
        var user = User.Create(ValidEmail, ValidHash, Roles.Manager);
        user.Role.Should().Be(Roles.Manager);
    }

    [Fact]
    public void Create_WithInvalidRole_ShouldThrowDomainException()
    {
        var act = () => User.Create(ValidEmail, ValidHash, "SUPERUSER");
        act.Should().Throw<DomainException>().WithMessage("*not a valid role*");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyPasswordHash_ShouldThrowDomainException(string hash)
    {
        var act = () => User.Create(ValidEmail, hash);
        act.Should().Throw<DomainException>();
    }


    [Fact]
    public void UpdateShippingAddress_ShouldReplaceAddress()
    {
        var user = User.Create(ValidEmail, ValidHash);
        var address = Address.Create("Rua Dom Dinis", "Porto", "4510-241", "Portugal");

        user.UpdateShippingAddress(address);

        user.ShippingAddress.Should().Be(address);
    }


    [Fact]
    public void UpdatePassword_ValidHash_ShouldUpdate()
    {
        var user = User.Create(ValidEmail, ValidHash);
        user.UpdatePassword("$newHash$");
        user.PasswordHash.Should().Be("$newHash$");
    }

    [Fact]
    public void UpdatePassword_EmptyHash_ShouldThrowDomainException()
    {
        var user = User.Create(ValidEmail, ValidHash);
        var act = () => user.UpdatePassword("");
        act.Should().Throw<DomainException>();
    }

}