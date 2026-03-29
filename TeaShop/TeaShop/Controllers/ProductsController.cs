using Microsoft.AspNetCore.Mvc;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController(IProductRepository productRepository) : ControllerBase
    {
        private readonly IProductRepository _productRepository = productRepository;

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
        {
            var products = await _productRepository.GetAllActive();
            return Ok(products);
        }

        [HttpGet("category/{categoryId:int}")]
        public async Task<IActionResult> GetByCategoryId(int categoryId)
        {
            var products = await _productRepository.GetByCategoryId(categoryId);
            return Ok(products);
        }

        [HttpGet("{externalId:guid}")]
        public async Task<IActionResult> GetByExternalId(Guid externalId)
        {
            var product = await _productRepository.GetByExternalId(externalId);
            if (product is null) return NotFound();
            return Ok(product);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Domains.Product product)
        {
            await _productRepository.Add(product);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = product.ExternalId }, product);
        }

        [HttpPut("{externalId:guid}")]
        public async Task<IActionResult> Update(Guid externalId, [FromBody] Domains.Product product)
        {
            var existing = await _productRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _productRepository.Update(product);
            return NoContent();
        }

        [HttpDelete("{externalId:guid}")]
        public async Task<IActionResult> Deactivate(Guid externalId)
        {
            var existing = await _productRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _productRepository.Deactivate(externalId);
            return NoContent();
        }
    }
}
