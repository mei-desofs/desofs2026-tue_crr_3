using System.ComponentModel.DataAnnotations;
namespace TeaShop.Application.UserManagement;

public sealed record CreateStaffRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(15), MaxLength(128)] string Password,
    [Required] string Role
);

public sealed record StaffCreatedResponse(Guid UserId, string Email, string Role);

public sealed record AddressResponse(string Street, string City, string PostalCode, string Country);