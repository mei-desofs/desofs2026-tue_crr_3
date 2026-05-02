using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TeaShop.Application.Products;
using TeaShop.Domain.Users;

namespace TeaShop.Presentation;

/// <summary>Manages product categories. All endpoints require the Admin role.</summary>
[ApiController]
[Route("api/categories")]
[Authorize(Roles = Roles.Admin)]
[Produces("application/json")]
public sealed class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>Creates a new product category.</summary>
    /// <param name="req">Category details. <c>Name</c> is required, max 100 chars, and must be unique.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="201">Category created successfully.</response>
    /// <response code="400">Validation failed — missing name or field exceeds max length.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="403">Caller does not have the Admin role.</response>
    /// <response code="409">A category with the same name already exists.</response>
    [HttpPost]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateCategoryRequest req, CancellationToken ct)
    {
        var response = await _categoryService.CreateAsync(req, User, ct);
        return CreatedAtAction(nameof(Create), new { id = response.Id }, response);
    }

    /// <summary>Partially updates an existing product category.</summary>
    /// <param name="categoryId">The ID of the category to update. Must not be an empty GUID.</param>
    /// <param name="req">Fields to update. Only non-null fields are applied.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Category updated successfully.</response>
    /// <response code="400">Validation failed or <paramref name="categoryId"/> is an empty GUID.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="403">Caller does not have the Admin role.</response>
    /// <response code="404">No category found with the given ID.</response>
    /// <response code="409">A category with the requested name already exists.</response>
    [HttpPatch("{categoryId}")]
    [ProducesResponseType(typeof(CategoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(Guid categoryId, UpdateCategoryRequest req, CancellationToken ct)
    {
        if (categoryId == Guid.Empty)
            return BadRequest("Category ID must not be an empty GUID.");

        var response = await _categoryService.UpdateAsync(categoryId, req, User, ct);
        return Ok(response);
    }

    /// <summary>Deletes an existing product category.</summary>
    /// <param name="categoryId">The ID of the category to delete. Must not be an empty GUID.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="204">Category deleted successfully.</response>
    /// <response code="400"><paramref name="categoryId"/> is an empty GUID.</response>
    /// <response code="401">No valid session token provided.</response>
    /// <response code="403">Caller does not have the Admin role.</response>
    /// <response code="404">No category found with the given ID.</response>
    [HttpDelete("{categoryId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid categoryId, CancellationToken ct)
    {
        if (categoryId == Guid.Empty)
            return BadRequest("Category ID must not be an empty GUID.");

        await _categoryService.DeleteAsync(categoryId, User, ct);
        return NoContent();
    }
}
