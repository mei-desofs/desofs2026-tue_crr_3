using Microsoft.AspNetCore.Mvc;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StockController(IStockRepository stockRepository) : ControllerBase
    {
        private readonly IStockRepository _stockRepository = stockRepository;

        [HttpGet("product/{productId:int}")]
        public async Task<IActionResult> GetByProductId(int productId)
        {
            var stock = await _stockRepository.GetByProductId(productId);
            if (stock is null) return NotFound();
            return Ok(stock);
        }

        [HttpGet("{externalId:guid}")]
        public async Task<IActionResult> GetByExternalId(Guid externalId)
        {
            var stock = await _stockRepository.GetByExternalId(externalId);
            if (stock is null) return NotFound();
            return Ok(stock);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Domains.Stock stock)
        {
            await _stockRepository.Add(stock);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = stock.ExternalId }, stock);
        }

        [HttpPut("{externalId:guid}")]
        public async Task<IActionResult> Update(Guid externalId, [FromBody] Domains.Stock stock)
        {
            var existing = await _stockRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _stockRepository.Update(stock);
            return NoContent();
        }
    }
}
