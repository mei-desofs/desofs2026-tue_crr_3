// tests/TeaShop.Tests.Unit/Domain/UserTests.cs
using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.IAM;
using TeaShop.Domain.Users;
using Xunit;

namespace TeaShop.Tests.Unit.Domain.Users;

public class UserTests
{
    private const string ValidEmail = "user@example.com";
    private const string ValidHash = "$2a$10$validhashvalue";


    [Fact]
    public void Create_ValidInputs_ShouldSucceed()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);

        user.Id.Should().NotBeEmpty();
        user.Email.Value.Should().Be(ValidEmail);
        user.Role.Should().Be(Roles.Customer);
        user.ShippingAddress.Should().BeNull();
    }

  

 

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_EmptyPasswordHash_ShouldThrowDomainException(string hash)
    {
        var act = () => User.CreateCustomer(ValidEmail, hash);
        act.Should().Throw<DomainException>();
    }


    [Fact]
    public void UpdateShippingAddress_ShouldReplaceAddress()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);
        var address = Address.Create("Rua Dom Dinis", "Porto", "4510-241", "Portugal");

        user.UpdateShippingAddress(address);

        user.ShippingAddress.Should().Be(address);
    }

    [Fact]
    public void UpdateShippingAddress_CalledTwice_ShouldReplaceWithLatest()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);
        user.UpdateShippingAddress(Address.Create("Old Street", "Porto", "4510-241", "Portugal"));
        var newer = Address.Create("New Street", "Lisbon", "1000-001", "Portugal");

        user.UpdateShippingAddress(newer);

        user.ShippingAddress.Should().Be(newer);
    }

    [Fact]
    public void RemoveShippingAddress_WithExistingAddress_ShouldSetToNull()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);
        user.UpdateShippingAddress(Address.Create("Rua Dom Dinis", "Porto", "4510-241", "Portugal"));

        user.RemoveShippingAddress();

        user.ShippingAddress.Should().BeNull();
    }

    [Fact]
    public void RemoveShippingAddress_WhenAlreadyNull_ShouldRemainNull()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);

        user.RemoveShippingAddress();

        user.ShippingAddress.Should().BeNull();
    }


    [Fact]
    public void UpdatePassword_ValidHash_ShouldUpdate()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);
        user.UpdatePassword(new PasswordHash ("$newHashPassword$"));
        user.PasswordHash.Value.Should().Be("$newHashPassword$");
    }

    [Fact]
    public void UpdatePassword_EmptyHash_ShouldThrowDomainException()
    {
        var user = User.CreateCustomer(ValidEmail, ValidHash);
        var act = () => user.UpdatePassword(new PasswordHash(""));
        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void CreateStaff_WithValidRole_CreatesUser()
    {
        var user = User.CreateStaff("admin@test.com", "hashed_password", Roles.Manager);

        Assert.Equal(Roles.Manager, user.Role);
    }

    [Fact]
    public void CreateStaff_WithCustomerRole_ThrowsDomainException()
    {
        var act = () => User.CreateStaff("hacker@test.com", "hashed_pw", Roles.Customer);

        var exception = Assert.Throws<DomainException>(act);
        Assert.Contains("not a valid staff role", exception.Message);
    }
}

