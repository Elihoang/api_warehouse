
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
            Description = p.Description,
            Image = p.Image,
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
            Description = product.Description,
            Image = product.Image,
            Price = product.Price,
            Time = product.Time,
            CategoryId = product.CategoryId, 
            SupplierId = product.SupplierId,
            CategoryName = product.Category?.CategoryName,
            SupplierName = product.Supplier?.SupplierName,
        };

        return Ok(dto);
    }

    // ===== CÁC ENDPOINT LỌC SẢN PHẨM =====

    [HttpGet("warehouse/{warehouseId}")]
    [SwaggerOperation(Summary = "Lấy danh sách sản phẩm theo kho")]
    public async Task<IActionResult> GetByWarehouseAsync(Guid warehouseId)
    {
        var products = await _service.GetByWarehouseIdAsync(warehouseId);

        var result = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Unit = p.Unit ?? "Cái",
            Description = p.Description,
            Image = p.Image,
            Price = p.Price,
            Time = p.Time,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            CategoryName = p.Category?.CategoryName,
            SupplierName = p.Supplier?.SupplierName,
        });

        return Ok(result);
    }

    [HttpGet("category/{categoryId}")]
    [SwaggerOperation(Summary = "Lấy danh sách sản phẩm theo danh mục")]
    public async Task<IActionResult> GetByCategoryAsync(Guid categoryId)
    {
        var products = await _service.GetByCategoryIdAsync(categoryId);

        var result = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Unit = p.Unit ?? "Cái",
            Description = p.Description,
            Image = p.Image,
            Price = p.Price,
            Time = p.Time,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            CategoryName = p.Category?.CategoryName,
            SupplierName = p.Supplier?.SupplierName,
        });

        return Ok(result);
    }

    [HttpGet("import-date")]
    [SwaggerOperation(Summary = "Lấy danh sách sản phẩm theo ngày nhập (1 ngày cụ thể)")]
    public async Task<IActionResult> GetByImportDateAsync([FromQuery] DateTime date)
    {
        var products = await _service.GetByImportDateAsync(date);

        var result = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Unit = p.Unit ?? "Cái",
            Description = p.Description,
            Image = p.Image,
            Price = p.Price,
            Time = p.Time,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            CategoryName = p.Category?.CategoryName,
            SupplierName = p.Supplier?.SupplierName,
        });

        return Ok(result);
    }

    [HttpGet("import-date-range")]
    [SwaggerOperation(Summary = "Lấy danh sách sản phẩm theo khoảng thời gian nhập")]
    public async Task<IActionResult> GetByImportDateRangeAsync(
        [FromQuery] DateTime startDate, 
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
            return BadRequest(new { message = "Ngày bắt đầu phải nhỏ hơn hoặc bằng ngày kết thúc" });

        var products = await _service.GetByImportDateRangeAsync(startDate, endDate);

        var result = products.Select(p => new ProductDto
        {
            ProductId = p.ProductId,
            ProductName = p.ProductName,
            Unit = p.Unit ?? "Cái",
            Description = p.Description,
            Image = p.Image,
            Price = p.Price,
            Time = p.Time,
            CategoryId = p.CategoryId,
            SupplierId = p.SupplierId,
            CategoryName = p.Category?.CategoryName,
            SupplierName = p.Supplier?.SupplierName,
        });

        return Ok(result);
    }

    // ===== CÁC ENDPOINT CRUD CHUẨN =====

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
            Description = dto.Description,
            Image = dto.Image,
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
            Description = product.Description,
            Image = product.Image,
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


        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy sản phẩm" });

        existing.ProductName = dto.ProductName;
        existing.CategoryId = dto.CategoryId;
        existing.SupplierId = dto.SupplierId;
        existing.Unit = dto.Unit;
        existing.Description = dto.Description;
        existing.Image = dto.Image;
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