using System.ComponentModel.DataAnnotations;

namespace TeaShop.Application.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(8), MaxLength(128)] string Password
);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public sealed record AuthResponse(string Token, DateTime ExpiresAt, string Role);
public sealed record UpdateAddressRequest(
    [Required] string Street,
    [Required] string City,
    [Required] string PostalCode,
    [Required] string Country
);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
