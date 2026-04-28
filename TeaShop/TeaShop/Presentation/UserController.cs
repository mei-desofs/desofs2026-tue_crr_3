using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application.Auth;
using TeaShop.Application.UserManagement;
using TeaShop.Domain.Users;

namespace TeaShop.Presentation.Controllers;

[ApiController]
[Route("api/users")]
public sealed class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [HttpPut("me/address")]
    [Authorize] 
    public async Task<IActionResult> UpdateMyAddress(UpdateAddressRequest req, CancellationToken ct)
    {
        if (HttpContext.Items["UserId"] is not Guid userId)
            return Unauthorized();

        await _userService.UpdateAddressAsync(userId, req, ct);
        return NoContent();
    }

    [HttpPost("staff")]
    [Authorize(Roles = Roles.Admin)] 
    public async Task<IActionResult> CreateStaff(CreateStaffRequest req, CancellationToken ct)
    {
        var newUserId = await _userService.CreateStaffAsync(req, ct);
        return Ok(new { UserId = newUserId });
    }
}