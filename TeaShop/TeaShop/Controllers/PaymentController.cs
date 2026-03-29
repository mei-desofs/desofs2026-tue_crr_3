using Microsoft.AspNetCore.Mvc;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController(IPaymentRepository paymentRepository) : ControllerBase
    {
        private readonly IPaymentRepository _paymentRepository = paymentRepository;

        [HttpGet("order/{orderId:int}")]
        public async Task<IActionResult> GetByOrderId(int orderId)
        {
            var payment = await _paymentRepository.GetByOrderId(orderId);
            if (payment is null) return NotFound();
            return Ok(payment);
        }

        [HttpGet("{externalId:guid}")]
        public async Task<IActionResult> GetByExternalId(Guid externalId)
        {
            var payment = await _paymentRepository.GetByExternalId(externalId);
            if (payment is null) return NotFound();
            return Ok(payment);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Domains.Payment payment)
        {
            await _paymentRepository.Add(payment);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = payment.ExternalId }, payment);
        }

        [HttpPut("{externalId:guid}")]
        public async Task<IActionResult> Update(Guid externalId, [FromBody] Domains.Payment payment)
        {
            var existing = await _paymentRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _paymentRepository.Update(payment);
            return NoContent();
        }
    }
}
