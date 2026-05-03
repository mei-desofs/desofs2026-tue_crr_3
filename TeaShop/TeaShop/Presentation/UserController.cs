using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application.Auth;
using TeaShop.Application.UserManagement;
using TeaShop.Domain.Users;

namespace TeaShop.Presentation.Controllers;

/// <summary>Manages user accounts and shipping addresses.</summary>
[ApiController]
[Route("api/users")]
[Produces("application/json")]
public sealed class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    /// <summary>Returns the shipping address of the authenticated user.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Address returned successfully.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="404">User has no address on file.</response>
    [HttpGet("me/address")]
    [Authorize]
    [ProducesResponseType(typeof(AddressResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetMyAddress(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var address = await _userService.GetAddressAsync(userId, ct);

        if (address is null)
            return NotFound("No address on file.");

        return Ok(address);
    }

    /// <summary>Sets or replaces the shipping address of the authenticated user.</summary>
    /// <param name="req">Full address details. All fields are required.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Address saved successfully.</response>
    /// <response code="400">Validation failed — one or more fields are missing or exceed max length.</response>
    /// <response code="401">No valid session token provided.</response>
    [HttpPut("me/address")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateMyAddress(UpdateAddressRequest req, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await _userService.UpdateAddressAsync(userId, req, ct);
        return NoContent();
    }

    /// <summary>Removes the shipping address of the authenticated user.</summary>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Address removed successfully.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="404">User has no address on file.</response>
    [HttpDelete("me/address")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteMyAddress(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        await _userService.RemoveAddressAsync(userId, ct);
        return NoContent();
    }


    /// <summary>Creates a staff account. Admin only.</summary>
    /// <param name="req">Staff account details including role.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Staff account created successfully.</response>
    /// <response code="400">Validation failed or role is invalid.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="403">Caller does not have the Admin role.</response>
    /// <response code="409">A user with this email already exists.</response>
    [HttpPost("staff")]
    [Authorize(Roles = Roles.Admin)]
    [ProducesResponseType(typeof(StaffCreatedResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateStaff(CreateStaffRequest req, CancellationToken ct)
    {
        var newUser = await _userService.CreateStaffAsync(req, ct);
        return Ok(newUser);
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}
