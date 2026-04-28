
using TeaShop.Domain.Exceptions;

namespace TeaShop.Domain.Users;

public sealed record Address(
    string Street,
    string City,
    string PostalCode,
    string Country)
{
    public static Address Create(string street, string city, string postalCode, string country)
    {
        if (string.IsNullOrWhiteSpace(street))
            throw new DomainException("Street is required.");
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("City is required.");
        if (string.IsNullOrWhiteSpace(postalCode))
            throw new DomainException("Postal code is required.");
        if (string.IsNullOrWhiteSpace(country))
            throw new DomainException("Country is required.");

        if (street.Length > 200)
            throw new DomainException("Street exceeds maximum length.");
        if (city.Length > 100)
            throw new DomainException("City exceeds maximum length.");
        if (postalCode.Length > 20)
            throw new DomainException("Postal code exceeds maximum length.");
        if (country.Length > 100)
            throw new DomainException("Country exceeds maximum length.");

        return new Address(
            street.Trim(),
            city.Trim(),
            postalCode.Trim(),
            country.Trim());
    }
}