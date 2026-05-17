using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application.Orders;
using TeaShop.Application.Orders.DTOs;
using TeaShop.Domain.Exceptions;
using TeaShop.Domain.Users;

namespace TeaShop.Presentation.Controllers;

[ApiController]
[Route("api/orders")]
[Produces("application/json")]
public sealed class OrderController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderRequest request,
        CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result = await _orderService.CreateAsync(userId, request, ct);
            return CreatedAtAction(nameof(CreateOrder), new { id = result.Id }, result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMyOrders(CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        var result = await _orderService.GetMyOrdersAsync(userId, ct);
        return Ok(result);
    }
    [HttpPatch("{id}/cancel")]
    [Authorize]

    public async Task<IActionResult> CancelOrder(Guid id, CancellationToken ct)
    {
        if (!TryGetUserId(out var userId))
            return Unauthorized();

        try
        {
            var result = await _orderService.CancelAsync(userId, id, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> GetAllOrders(CancellationToken ct)
    {
        var result = await _orderService.GetAllOrdersAsync(ct);
        return Ok(result);
    }

    [HttpPut("{id}/status")]
    [Authorize(Roles = Roles.Admin)]
    public async Task<IActionResult> UpdateOrderStatus(
        Guid id,
        [FromBody] UpdateOrderStatusRequest request,
        CancellationToken ct)
    {
        try
        {
            var result = await _orderService.UpdateOrderStatusAsync(id, request, ct);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool TryGetUserId(out Guid userId) =>
        Guid.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out userId);
}