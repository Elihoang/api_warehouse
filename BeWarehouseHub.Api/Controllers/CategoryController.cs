// BeWarehouseHub.Api/Controllers/CategoryController.cs
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Category;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _service;

    public CategoryController(CategoryService service)
    {
        _service = service;
    }

    // GET: api/Category
    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả danh mục")]
    public async Task<IActionResult> GetAllAsync()
    {
        var categories = await _service.GetAllCateAsync();

        if (categories == null || !categories.Any())
            return NoContent();
        
        var result = categories.Select(c => new CategoryDto
        {
            CategoryId = c.CategoryId,
            CategoryName = c.CategoryName,
            ProductCount = c.Products?.Count ?? 0
        });

        return Ok(result);
    }

    // GET: api/Category/{id}
    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy danh mục theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var category = await _service.GetCateByIdAsync(id);

        if (category == null)
            return NotFound(new { message = "Không tìm thấy danh mục" });

        var dto = new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ProductCount = category.Products?.Count ?? 0
        };

        return Ok(dto);
    }

    // POST: api/Category
    [HttpPost]
    [SwaggerOperation(Summary = "Tạo mới danh mục")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var category = new Category
        {
            CategoryId = Guid.NewGuid(), 
            CategoryName = dto.CategoryName
        };

        await _service.AddCateAsync(category);

        var result = new CategoryDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            ProductCount = 0
        };

        return Ok(result);

    }

    // PUT: api/Category/{id}
    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật danh mục")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateCategoryDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.CategoryId)
            return BadRequest(new { message = "Id không khớp" });

        var existing = await _service.GetCateByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy danh mục" });

        existing.CategoryName = dto.CategoryName;
        await _service.UpdateCateAsync(existing);

        return NoContent();
    }

    // DELETE: api/Category/{id}
    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa danh mục")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var category = await _service.GetCateByIdAsync(id);
        if (category == null)
            return NotFound(new { message = "Không tìm thấy danh mục" });

        await _service.DeleteCateAsync(category);
        return NoContent();
    }
}