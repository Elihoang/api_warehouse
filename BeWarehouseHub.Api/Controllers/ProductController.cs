
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Product;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductController : ControllerBase
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả sản phẩm")]
    public async Task<IActionResult> GetAllAsync()
    {
        var products = await _service.GetAllAsync();

        var result = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Unit = p.Unit ?? "Cái",
            CategoryId = p.CategoryId, 
            SupplierId = p.SupplierId,
            Price = p.Price,
            Time = p.Time,
            CategoryName = p.Category?.CategoryName,
            SupplierName = p.Supplier?.SupplierName,
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy sản phẩm theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm" });

        var dto = new ProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Unit = product.Unit ?? "Cái",
            Price = product.Price,
            Time = product.Time,
            CategoryId = product.CategoryId, 
            SupplierId = product.SupplierId,
            CategoryName = product.Category?.CategoryName,
            SupplierName = product.Supplier?.SupplierName,
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo mới sản phẩm")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var product = new Product
        {
            ProductId = Guid.NewGuid(),
            ProductName = dto.ProductName,
            CategoryId = dto.CategoryId,
            SupplierId = dto.SupplierId,
            Unit = dto.Unit,
            Price = dto.Price,
            Time = DateTime.UtcNow
        };

        await _service.AddAsync(product);

        var result = new ProductDto
        {
            ProductId = product.ProductId,
            ProductName = product.ProductName,
            Unit = product.Unit!,
            Price = product.Price,
            Time = product.Time,
            CategoryId = product.CategoryId, 
            SupplierId = product.SupplierId,
            CategoryName = product.Category?.CategoryName,
            SupplierName = product.Supplier?.SupplierName,
        };

        return Ok(result);
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật sản phẩm")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.ProductId)
            return BadRequest(new { message = "Id không khớp" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm" });

        existing.ProductName = dto.ProductName;
        existing.CategoryId = dto.CategoryId;
        existing.SupplierId = dto.SupplierId;
        existing.Unit = dto.Unit;
        existing.Price = dto.Price;

        await _service.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa sản phẩm")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var product = await _service.GetByIdAsync(id);
        if (product == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm" });

        await _service.DeleteAsync(product);
        return NoContent();
    }
}