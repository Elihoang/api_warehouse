using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Warehouse;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class WarehouseController : ControllerBase
{
    private readonly WarehouseService _service;

    public WarehouseController(WarehouseService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả kho")]
    public async Task<IActionResult> GetAllAsync()
    {
        var warehouses = await _service.GetAllAsync();

        var result = warehouses.Select(w => new WarehouseDto
        {
            WarehouseId = w.WarehouseId,
            WarehouseName = w.WarehouseName,
            Location = w.Location ?? string.Empty,
            ProductCount = w.Stocks?.Where(s => s.Quantity > 0).Select(s => s.ProductId).Distinct().Count() ?? 0,
            TotalStockQuantity = w.Stocks?.Sum(s => s.Quantity) ?? 0,
            ImportReceiptCount = w.ImportReceipts?.Count ?? 0,
            ExportReceiptCount = w.ExportReceipts?.Count ?? 0
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation("Lấy thông tin kho theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var warehouse = await _service.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound(new { message = "Không tìm thấy kho" });

        var dto = new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseName = warehouse.WarehouseName,
            Location = warehouse.Location ?? string.Empty,
            ProductCount = warehouse.Stocks?.Where(s => s.Quantity > 0).Select(s => s.ProductId).Distinct().Count() ?? 0,
            TotalStockQuantity = warehouse.Stocks?.Sum(s => s.Quantity) ?? 0,
            ImportReceiptCount = warehouse.ImportReceipts?.Count ?? 0,
            ExportReceiptCount = warehouse.ExportReceipts?.Count ?? 0
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo mới kho")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateWarehouseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var warehouse = new Warehouse
        {
            WarehouseId = Guid.NewGuid(),
            WarehouseName = dto.WarehouseName,
            Location = dto.Location
        };

        await _service.AddAsync(warehouse);

        var result = new WarehouseDto
        {
            WarehouseId = warehouse.WarehouseId,
            WarehouseName = warehouse.WarehouseName,
            Location = warehouse.Location ?? string.Empty,
            ProductCount = 0,
            TotalStockQuantity = 0,
            ImportReceiptCount = 0,
            ExportReceiptCount = 0
        };

        return Ok(result);
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật thông tin kho")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateWarehouseDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.WarehouseId)
            return BadRequest(new { message = "Id không khớp" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy kho" });

        existing.WarehouseName = dto.WarehouseName;
        existing.Location = dto.Location;

        await _service.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa kho (chỉ xóa được khi không còn phiếu nhập/xuất và tồn kho)")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var warehouse = await _service.GetByIdAsync(id);
        if (warehouse == null)
            return NotFound(new { message = "Không tìm thấy kho" });

        // Có thể thêm kiểm tra không cho xóa nếu còn dữ liệu
        if ((warehouse.Stocks?.Any() ?? false) || 
            (warehouse.ImportReceipts?.Any() ?? false) || 
            (warehouse.ExportReceipts?.Any() ?? false))
        {
            return BadRequest(new { message = "Không thể xóa kho đang có tồn kho hoặc phiếu nhập/xuất" });
        }

        await _service.DeleteAsync(warehouse);
        return NoContent();
    }
}