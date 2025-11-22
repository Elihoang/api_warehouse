
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Supplier;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SupplierController : ControllerBase
{
    private readonly SupplierService _service;

    public SupplierController(SupplierService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả nhà cung cấp")]
    public async Task<IActionResult> GetAllAsync()
    {
        var suppliers = await _service.GetAllAsync();

        var result = suppliers.Select(s => new SupplierDto
        {
            SupplierId = s.SupplierId,
            SupplierName = s.SupplierName,
            Phone = s.Phone ?? string.Empty,
            Address = s.Address ?? string.Empty,
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy nhà cung cấp theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var supplier = await _service.GetByIdAsync(id);
        if (supplier == null)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp" });

        var dto = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone ?? string.Empty,
            Address = supplier.Address ?? string.Empty
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo mới nhà cung cấp")]
    public async Task<IActionResult> CreateAsync([FromBody] SupplierDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            SupplierName = dto.SupplierName,
            Phone = dto.Phone,
            Address = dto.Address
        };

        await _service.AddAsync(supplier);

        var result = new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            SupplierName = supplier.SupplierName,
            Phone = supplier.Phone ?? string.Empty,
            Address = supplier.Address ?? string.Empty,
        };

        return Ok(result);
    }

    [HttpPatch("{id}")] // hoặc [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Cập nhật nhà cung cấp")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] SupplierDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.SupplierId)
            return BadRequest(new { message = "Id không khớp" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp" });

        existing.SupplierName = dto.SupplierName;
        existing.Phone = dto.Phone;
        existing.Address = dto.Address;

        await _service.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa nhà cung cấp")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var supplier = await _service.GetByIdAsync(id);
        if (supplier == null)
            return NotFound(new { message = "Không tìm thấy nhà cung cấp" });

        await _service.DeleteAsync(supplier);
        return NoContent();
    }
}