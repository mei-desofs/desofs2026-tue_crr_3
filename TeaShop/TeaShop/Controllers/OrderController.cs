using Microsoft.AspNetCore.Mvc;
using TeaShop.Enums;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrderController(IOrderRepository orderRepository) : ControllerBase
    {
        private readonly IOrderRepository _orderRepository = orderRepository;

        [HttpGet("user/{userId:int}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var orders = await _orderRepository.GetByUserId(userId);
            return Ok(orders);
        }

        [HttpGet("status/{status}")]
        public async Task<IActionResult> GetByStatus(OrderStatus status)
        {
            var orders = await _orderRepository.GetByStatus(status);
            return Ok(orders);
        }

        [HttpGet("{externalId:guid}")]
        public async Task<IActionResult> GetByExternalId(Guid externalId)
        {
            var order = await _orderRepository.GetByExternalId(externalId);
            if (order is null) return NotFound();
            return Ok(order);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Domains.Order order)
        {
            await _orderRepository.Add(order);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = order.ExternalId }, order);
        }

        [HttpPut("{externalId:guid}")]
        public async Task<IActionResult> Update(Guid externalId, [FromBody] Domains.Order order)
        {
            var existing = await _orderRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _orderRepository.Update(order);
            return NoContent();
        }
    }
}
