using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.ProductBatch;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductBatchController : ControllerBase
{
    private readonly ProductBatchService _service;

    public ProductBatchController(ProductBatchService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả lô hàng")]
    public async Task<IActionResult> GetAllAsync()
    {
        var batches = await _service.GetAllAsync();

        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy lô hàng theo ID")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var batch = await _service.GetByIdAsync(id);
        if (batch == null)
            return NotFound(new { message = "Không tìm thấy lô hàng" });

        return Ok(MapToDto(batch));
    }

    [HttpGet("batch-number/{batchNumber}")]
    [SwaggerOperation(Summary = "Lấy lô hàng theo mã lô")]
    public async Task<IActionResult> GetByBatchNumberAsync(string batchNumber)
    {
        var batch = await _service.GetByBatchNumberAsync(batchNumber);
        if (batch == null)
            return NotFound(new { message = $"Không tìm thấy lô hàng với mã {batchNumber}" });

        return Ok(MapToDto(batch));
    }

    [HttpGet("product/{productId}")]
    [SwaggerOperation(Summary = "Lấy danh sách lô hàng theo sản phẩm")]
    public async Task<IActionResult> GetByProductIdAsync(Guid productId)
    {
        var batches = await _service.GetBatchesByProductIdAsync(productId);
        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [SwaggerOperation(Summary = "Lấy danh sách lô hàng theo kho")]
    public async Task<IActionResult> GetByWarehouseIdAsync(Guid warehouseId)
    {
        var batches = await _service.GetBatchesByWarehouseIdAsync(warehouseId);
        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpGet("expiring")]
    [SwaggerOperation(Summary = "Lấy lô hàng sắp hết hạn")]
    public async Task<IActionResult> GetExpiringBatchesAsync([FromQuery] int daysUntilExpiry = 30)
    {
        var batches = await _service.GetExpiringBatchesAsync(daysUntilExpiry);
        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [SwaggerOperation(Summary = "Lấy lô hàng theo trạng thái (Available, Expired, Recalled, Sold)")]
    public async Task<IActionResult> GetByStatusAsync(string status)
    {
        var batches = await _service.GetBatchesByStatusAsync(status);
        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpGet("fifo")]
    [SwaggerOperation(Summary = "Lấy lô hàng FIFO (sắp hết hạn trước) theo sản phẩm và kho")]
    public async Task<IActionResult> GetFIFOBatchesAsync([FromQuery] Guid productId, [FromQuery] Guid warehouseId)
    {
        var batches = await _service.GetAvailableBatchesFIFOAsync(productId, warehouseId);
        var result = batches.Select(b => MapToDto(b));
        return Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo lô hàng mới")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateProductBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var batch = new ProductBatch
            {
                BatchId = Guid.NewGuid(),
                BatchNumber = dto.BatchNumber,
                ProductId = dto.ProductId,
                Product = null!,
                WarehouseId = dto.WarehouseId,
                Warehouse = null!,
                ManufactureDate = dto.ManufactureDate,
                ExpiryDate = dto.ExpiryDate,
                Quantity = dto.Quantity,
                CostPrice = dto.CostPrice,
                Note = dto.Note,
                Status = "Available",
                CreatedAt = DateTime.UtcNow,
                ImportDetails = new List<ImportDetail>(),
                ExportDetails = new List<ExportDetail>()
            };

            await _service.AddAsync(batch);

            // Reload to get navigation properties
            var created = await _service.GetByIdAsync(batch.BatchId);
            
            // Return 201 Created with the created object
            Response.Headers.Append("Location", $"/api/ProductBatch/{batch.BatchId}");
            return StatusCode(201, MapToDto(created!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật lô hàng")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductBatchDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy lô hàng" });

        if (dto.Quantity.HasValue)
            existing.Quantity = dto.Quantity.Value;

        if (!string.IsNullOrEmpty(dto.Status))
            existing.Status = dto.Status;

        if (dto.Note != null)
            existing.Note = dto.Note;

        await _service.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa lô hàng")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var batch = await _service.GetByIdAsync(id);
        if (batch == null)
            return NotFound(new { message = "Không tìm thấy lô hàng" });

        await _service.DeleteAsync(batch);
        return NoContent();
    }

    [HttpPost("update-expired")]
    [SwaggerOperation(Summary = "Tự động cập nhật status cho lô hàng hết hạn")]
    public async Task<IActionResult> UpdateExpiredBatchesAsync()
    {
        await _service.UpdateExpiredBatchesAsync();
        return Ok(new { message = "Đã cập nhật trạng thái lô hàng hết hạn" });
    }

    // Helper method to map entity to DTO
    private ProductBatchDto MapToDto(ProductBatch batch)
    {
        var now = DateTime.UtcNow;
        var daysUntilExpiry = (batch.ExpiryDate - now).Days;

        return new ProductBatchDto
        {
            BatchId = batch.BatchId,
            BatchNumber = batch.BatchNumber,
            ProductId = batch.ProductId,
            ProductName = batch.Product?.ProductName,
            WarehouseId = batch.WarehouseId,
            WarehouseName = batch.Warehouse?.WarehouseName,
            ManufactureDate = batch.ManufactureDate,
            ExpiryDate = batch.ExpiryDate,
            Quantity = batch.Quantity,
            Status = batch.Status,
            CostPrice = batch.CostPrice,
            Note = batch.Note,
            CreatedAt = batch.CreatedAt,
            DaysUntilExpiry = daysUntilExpiry,
            IsExpiring = daysUntilExpiry <= 30 && daysUntilExpiry >= 0,
            IsExpired = batch.ExpiryDate < now
        };
    }
}
