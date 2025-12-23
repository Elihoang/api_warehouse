using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.AutoReorderSettings;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AutoReorderSettingsController : ControllerBase
{
    private readonly AutoReorderSettingsService _service;

    public AutoReorderSettingsController(AutoReorderSettingsService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy tất cả cấu hình tự động đặt hàng")]
    public async Task<IActionResult> GetAllAsync()
    {
        var settings = await _service.GetAllAsync();
        var result = settings.Select(s => MapToDto(s));
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy cấu hình theo ID")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var setting = await _service.GetByIdAsync(id);
        if (setting == null)
            return NotFound(new { message = "Không tìm thấy cấu hình" });

        return Ok(MapToDto(setting));
    }

    [HttpGet("product-warehouse")]
    [SwaggerOperation(Summary = "Lấy cấu hình theo sản phẩm và kho")]
    public async Task<IActionResult> GetByProductAndWarehouseAsync([FromQuery] Guid productId, [FromQuery] Guid warehouseId)
    {
        var setting = await _service.GetByProductAndWarehouseAsync(productId, warehouseId);
        if (setting == null)
            return NotFound(new { message = "Không tìm thấy cấu hình" });

        return Ok(MapToDto(setting));
    }

    [HttpGet("enabled")]
    [SwaggerOperation(Summary = "Lấy tất cả cấu hình đang bật")]
    public async Task<IActionResult> GetEnabledAsync()
    {
        var settings = await _service.GetEnabledSettingsAsync();
        var result = settings.Select(s => MapToDto(s));
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [SwaggerOperation(Summary = "Lấy cấu hình theo kho")]
    public async Task<IActionResult> GetByWarehouseAsync(Guid warehouseId)
    {
        var settings = await _service.GetByWarehouseIdAsync(warehouseId);
        var result = settings.Select(s => MapToDto(s));
        return Ok(result);
    }

    [HttpGet("check-reorder-needs")]
    [SwaggerOperation(Summary = "Kiểm tra sản phẩm cần đặt hàng")]
    [SwaggerResponse(200, "Danh sách sản phẩm cần đặt hàng")]
    public async Task<IActionResult> CheckReorderNeedsAsync()
    {
        var recommendations = await _service.CheckReorderNeedsAsync();
        
        return Ok(new
        {
            totalProducts = recommendations.Count,
            recommendations = recommendations.Select(r => new
            {
                r.ProductId,
                r.ProductName,
                r.WarehouseId,
                r.WarehouseName,
                r.CurrentStock,
                r.ReorderPoint,
                r.RecommendedQuantity,
                r.MinStockLevel,
                r.MaxStockLevel,
                r.LeadTimeDays,
                r.SuggestedOrderDate,
                r.ExpectedDeliveryDate,
                urgency = r.CurrentStock < r.MinStockLevel ? "Critical" : "Normal"
            })
        });
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo cấu hình mới")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateAutoReorderSettingsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var setting = new AutoReorderSettings
            {
                SettingId = Guid.NewGuid(),
                ProductId = dto.ProductId,
                Product = null!,
                WarehouseId = dto.WarehouseId,
                Warehouse = null!,
                MinStockLevel = dto.MinStockLevel,
                ReorderPoint = dto.ReorderPoint,
                ReorderQuantity = dto.ReorderQuantity,
                MaxStockLevel = dto.MaxStockLevel,
                IsAutoReorderEnabled = dto.IsAutoReorderEnabled,
                LeadTimeDays = dto.LeadTimeDays,
                CreatedAt = DateTime.UtcNow
            };

            await _service.AddAsync(setting);

            var created = await _service.GetByIdAsync(setting.SettingId);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = setting.SettingId }, MapToDto(created!));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật cấu hình")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateAutoReorderSettingsDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy cấu hình" });

        if (dto.MinStockLevel.HasValue)
            existing.MinStockLevel = dto.MinStockLevel.Value;

        if (dto.ReorderPoint.HasValue)
            existing.ReorderPoint = dto.ReorderPoint.Value;

        if (dto.ReorderQuantity.HasValue)
            existing.ReorderQuantity = dto.ReorderQuantity.Value;

        if (dto.MaxStockLevel.HasValue)
            existing.MaxStockLevel = dto.MaxStockLevel.Value;

        if (dto.IsAutoReorderEnabled.HasValue)
            existing.IsAutoReorderEnabled = dto.IsAutoReorderEnabled.Value;

        if (dto.LeadTimeDays.HasValue)
            existing.LeadTimeDays = dto.LeadTimeDays.Value;

        try
        {
            await _service.UpdateAsync(existing);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa cấu hình")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var setting = await _service.GetByIdAsync(id);
        if (setting == null)
            return NotFound(new { message = "Không tìm thấy cấu hình" });

        await _service.DeleteAsync(setting);
        return NoContent();
    }

    [HttpPost("suggest")]
    [SwaggerOperation(Summary = "Đề xuất cấu hình tự động dựa trên lịch sử")]
    [SwaggerResponse(200, "Cấu hình được đề xuất")]
    public async Task<IActionResult> SuggestSettingsAsync(
        [FromQuery] Guid productId,
        [FromQuery] Guid warehouseId,
        [FromQuery] int lookbackDays = 90)
    {
        try
        {
            var suggestedSettings = await _service.SuggestSettingsAsync(productId, warehouseId, lookbackDays);
            
            return Ok(new
            {
                message = $"Đề xuất dựa trên {lookbackDays} ngày lịch sử",
                settings = MapToDto(suggestedSettings),
                note = "Đây chỉ là đề xuất, bạn nên xem xét và điều chỉnh trước khi áp dụng"
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("formulas")]
    [SwaggerOperation(Summary = "Lấy công thức tính toán")]
    public IActionResult GetFormulas()
    {
        return Ok(new
        {
            reorderPoint = new
            {
                formula = "Reorder Point = Lead Time Demand + Safety Stock",
                description = "Điểm đặt hàng lại = Nhu cầu trong thời gian giao hàng + Tồn kho an toàn"
            },
            safetyStock = new
            {
                formula = "Safety Stock = Z-score × Standard Deviation × √(Lead Time)",
                description = "Tồn kho an toàn = 1.65 × Độ lệch chuẩn (95% service level)",
                zScore = 1.65,
                serviceLevel = "95%"
            },
            example = new
            {
                avgDailyDemand = 50,
                leadTimeDays = 7,
                stdDev = 10,
                safetyStock = "1.65 × 10 = 17 units",
                leadTimeDemand = "50 × 7 = 350 units",
                reorderPoint = "350 + 17 = 367 units",
                reorderQuantity = "Trung bình 30 ngày = 50 × 30 = 1,500 units"
            }
        });
    }

    // Helper method
    private AutoReorderSettingsDto MapToDto(AutoReorderSettings setting)
    {
        // Would need StockRepository to get current stock
        // For now, return 0
        int currentStock = 0;
        bool shouldReorder = currentStock <= setting.ReorderPoint;

        return new AutoReorderSettingsDto
        {
            SettingId = setting.SettingId,
            ProductId = setting.ProductId,
            ProductName = setting.Product?.ProductName,
            WarehouseId = setting.WarehouseId,
            WarehouseName = setting.Warehouse?.WarehouseName,
            MinStockLevel = setting.MinStockLevel,
            ReorderPoint = setting.ReorderPoint,
            ReorderQuantity = setting.ReorderQuantity,
            MaxStockLevel = setting.MaxStockLevel,
            IsAutoReorderEnabled = setting.IsAutoReorderEnabled,
            LeadTimeDays = setting.LeadTimeDays,
            CreatedAt = setting.CreatedAt,
            UpdatedAt = setting.UpdatedAt,
            CurrentStock = currentStock,
            ShouldReorder = shouldReorder
        };
    }
}
