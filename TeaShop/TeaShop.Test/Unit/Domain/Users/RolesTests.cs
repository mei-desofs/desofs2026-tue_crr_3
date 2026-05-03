// tests/TeaShop.Tests.Unit/Domain/RolesTests.cs
using FluentAssertions;
using TeaShop.Domain.Users;
using Xunit;
namespace TeaShop.Tests.Unit.Domain;

public class RolesTests
{
    [Theory]
    [InlineData(Roles.Customer)]
    [InlineData(Roles.Manager)]
    [InlineData(Roles.Admin)]
    public void IsValid_KnownRoles_ShouldReturnTrue(string role)
    {
        Roles.IsValid(role).Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("USER")]
    [InlineData("superadmin")]
    public void IsValid_UnknownRoles_ShouldReturnFalse(string role)
    {
        Roles.IsValid(role).Should().BeFalse();
    }
}