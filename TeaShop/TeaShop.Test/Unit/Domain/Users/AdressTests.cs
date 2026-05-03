using FluentAssertions;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;
using Xunit;

namespace TeaShop.Tests.Unit.Domain.Users;

public class AddressTests
{
    [Fact]
    public void Create_ValidAddress_ShouldSucceed()
    {
        var address = Address.Create("Avenida dos Aliados, 12", "Porto", "4000-064", "Portugal");

        address.Street.Should().Be("Avenida dos Aliados, 12");
        address.City.Should().Be("Porto");
        address.PostalCode.Should().Be("4000-064");
        address.Country.Should().Be("Portugal");
    }

    [Fact]
    public void Create_ShouldTrimWhitespace()
    {
        var address = Address.Create("  Avenida dos Aliados, 12  ", "  Porto  ", "4000-064", "Portugal");
        address.Street.Should().Be("Avenida dos Aliados, 12");
        address.City.Should().Be("Porto");
    }

    [Theory]
    [InlineData("", "Porto", "4000-064", "Portugal", "Street is required.")]
    [InlineData("Avenida dos Aliados", "", "4000-064", "Portugal", "City is required.")]
    [InlineData("Avenida dos Aliados", "Porto", "", "Portugal", "Postal code is required.")]
    [InlineData("Avenida dos Aliados", "Porto", "4000-064", "", "Country is required.")]
    public void Create_MissingField_ShouldThrowDomainException(
        string street, string city, string postalCode, string country, string expectedMessage)
    {
        var act = () => Address.Create(street, city, postalCode, country);
        act.Should().Throw<DomainException>().WithMessage(expectedMessage);
    }

    [Fact]
    public void Create_StreetTooLong_ShouldThrowDomainException()
    {
        var longStreet = new string('a', 201);
        var act = () => Address.Create(longStreet, "Porto", "4000-064", "Portugal");
        act.Should().Throw<DomainException>().WithMessage("Street exceeds maximum length.");
    }

    [Fact]
    public void Create_PostalCodeTooLong_ShouldThrowDomainException()
    {
        var longPostalCode = new string('1', 21);
        var act = () => Address.Create("Rua Augusta", "Lisboa", longPostalCode, "Portugal");
        act.Should().Throw<DomainException>().WithMessage("Postal code exceeds maximum length.");
    }

    [Fact]
    public void TwoAddresses_WithSameValues_ShouldBeEqual()
    {
        var a = Address.Create("Rua Augusta, 100", "Lisboa", "1100-048", "Portugal");
        var b = Address.Create("Rua Augusta, 100", "Lisboa", "1100-048", "Portugal");
        a.Should().Be(b);
    }

    [Fact]
    public void TwoAddresses_WithDifferentValues_ShouldNotBeEqual()
    {
        var a = Address.Create("Avenida dos Aliados, 12", "Porto", "4000-064", "Portugal");
        var b = Address.Create("Rua Augusta, 100", "Lisboa", "1100-048", "Portugal");
        a.Should().NotBe(b);
    }
}