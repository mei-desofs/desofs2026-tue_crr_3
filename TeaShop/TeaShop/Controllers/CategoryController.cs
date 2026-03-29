using Microsoft.AspNetCore.Mvc;
using TeaShop.Repositories.Interfaces;

namespace TeaShop.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoryController(ICategoryRepository categoryRepository) : ControllerBase
    {
        private readonly ICategoryRepository _categoryRepository = categoryRepository;

        [HttpGet("active")]
        public async Task<IActionResult> GetAllActive()
        {
            var categories = await _categoryRepository.GetAllActive();
            return Ok(categories);
        }

        [HttpGet("{externalId:guid}")]
        public async Task<IActionResult> GetByExternalId(Guid externalId)
        {
            var category = await _categoryRepository.GetByExternalId(externalId);
            if (category is null) return NotFound();
            return Ok(category);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Domains.Category category)
        {
            await _categoryRepository.Add(category);
            return CreatedAtAction(nameof(GetByExternalId), new { externalId = category.ExternalId }, category);
        }

        [HttpPut("{externalId:guid}")]
        public async Task<IActionResult> Update(Guid externalId, [FromBody] Domains.Category category)
        {
            var existing = await _categoryRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _categoryRepository.Update(category);
            return NoContent();
        }

        [HttpDelete("{externalId:guid}")]
        public async Task<IActionResult> Deactivate(Guid externalId)
        {
            var existing = await _categoryRepository.GetByExternalId(externalId);
            if (existing is null) return NotFound();

            await _categoryRepository.Deactivate(externalId);
            return NoContent();
        }
    }
}
