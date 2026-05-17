using System.ComponentModel.DataAnnotations;

namespace TeaShop.Application.Auth;

public sealed record RegisterRequest(
    [Required, EmailAddress] string Email,
    [Required, MinLength(15), MaxLength(128)] string Password
);

public sealed record LoginRequest(
    [Required, EmailAddress] string Email,
    [Required] string Password
);

public sealed record AuthResponse(string Token, DateTime ExpiresAt, string Role);
public sealed record UpdateAddressRequest(
    [Required, MaxLength(200)] string Street,
    [Required, MaxLength(100)] string City,
    [Required, MaxLength(20)] string PostalCode,
    [Required, MaxLength(100)] string Country
);
public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
