using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application.Catalog;
using TeaShop.Domain.Exceptions;

namespace TeaShop.Presentation.Controllers;

[ApiController]
[Route("api/catalog")]
public sealed class CatalogController : ControllerBase
{
    private readonly CatalogService _service;

    public CatalogController(CatalogService service)
    {
        _service = service;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? categoryId,
        CancellationToken ct)
    {
        var result = await _service.GetAllAsync(categoryId, ct);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        try
        {
            var result = await _service.GetByIdAsync(id, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
    [HttpPost]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Create(
        [FromBody] CreateTeaRequestDto request,
        CancellationToken ct)
    {
        var result = await _service.CreateAsync(request, ct);
        return Created($"/api/catalog/{result.Id}", result);
    }
    [HttpPut("{id}")]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateTeaRequestDto request,
        CancellationToken ct)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, ct);
            return Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id}/image")]
    [Authorize(Policy = "ManagerOrAbove")]
    public async Task<IActionResult> UploadImage(
         Guid id,
         [FromForm] UploadTeaImageDto dto,
         CancellationToken ct)
    {
        try
        {
            await _service.UploadImageAsync(id, dto.File, ct);
            return Ok(new { message = "Image uploaded successfully." });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (DomainException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("{id}/image")]
    [AllowAnonymous]
    public async Task<IActionResult> GetImage(Guid id, CancellationToken ct)
    {
        try
        {
            var (fileBytes, mimeType, fileName) = await _service.GetImageAsync(id, ct);
            return File(fileBytes, mimeType, fileName);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (FileNotFoundException)
        {
            return NotFound(new { error = "Image file is missing from server." });
        }
    }

    [HttpDelete("{id}/image")]
    [Authorize(Policy = "ManagerOrAbove")] 
    public async Task<IActionResult> DeleteImage(Guid id, CancellationToken ct)
    {
        try
        {
            await _service.DeleteImageAsync(id, ct);
            return NoContent();
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }
}