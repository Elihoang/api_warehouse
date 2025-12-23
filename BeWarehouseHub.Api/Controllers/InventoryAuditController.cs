using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.InventoryAudit;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class InventoryAuditController : ControllerBase
{
    private readonly InventoryAuditService _service;

    public InventoryAuditController(InventoryAuditService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả phiếu kiểm kê")]
    public async Task<IActionResult> GetAllAsync()
    {
        var audits = await _service.GetAllAsync();
        var result = audits.Select(a => MapToDto(a, false));
        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy phiếu kiểm kê theo ID (không bao gồm details)")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var audit = await _service.GetByIdAsync(id);
        if (audit == null)
            return NotFound(new { message = "Không tìm thấy phiếu kiểm kê" });

        return Ok(MapToDto(audit, false));
    }

    [HttpGet("{id}/details")]
    [SwaggerOperation(Summary = "Lấy phiếu kiểm kê với chi tiết")]
    public async Task<IActionResult> GetWithDetailsAsync(Guid id)
    {
        var audit = await _service.GetAuditWithDetailsAsync(id);
        if (audit == null)
            return NotFound(new { message = "Không tìm thấy phiếu kiểm kê" });

        var dto = MapToDto(audit, true);
        return Ok(new
        {
            Audit = dto,
            Details = audit.InventoryAuditDetails.Select(d => MapDetailToDto(d))
        });
    }

    [HttpGet("warehouse/{warehouseId}")]
    [SwaggerOperation(Summary = "Lấy phiếu kiểm kê theo kho")]
    public async Task<IActionResult> GetByWarehouseIdAsync(Guid warehouseId)
    {
        var audits = await _service.GetAuditsByWarehouseIdAsync(warehouseId);
        var result = audits.Select(a => MapToDto(a, false));
        return Ok(result);
    }

    [HttpGet("status/{status}")]
    [SwaggerOperation(Summary = "Lấy phiếu kiểm kê theo trạng thái (InProgress, Completed, Cancelled)")]
    public async Task<IActionResult> GetByStatusAsync(string status)
    {
        var audits = await _service.GetAuditsByStatusAsync(status);
        var result = audits.Select(a => MapToDto(a, false));
        return Ok(result);
    }

    [HttpGet("{id}/variance")]
    [SwaggerOperation(Summary = "Lấy chi tiết kiểm kê có chênh lệch")]
    public async Task<IActionResult> GetDetailsWithVarianceAsync(Guid id)
    {
        var details = await _service.GetDetailsWithVarianceAsync(id);
        var result = details.Select(d => MapDetailToDto(d));
        return Ok(result);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo phiếu kiểm kê mới")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateInventoryAuditDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var audit = new InventoryAudit
            {
                AuditId = Guid.NewGuid(),
                AuditCode = dto.AuditCode,
                WarehouseId = dto.WarehouseId,
                Warehouse = null!,
                AuditDate = dto.AuditDate,
                CreatedByUserId = dto.CreatedByUserId,
                CreatedByUser = null!,
                Status = "InProgress",
                Note = dto.Note,
                CreatedAt = DateTime.UtcNow,
                InventoryAuditDetails = new List<InventoryAuditDetail>()
            };

            var created = await _service.CreateAuditAsync(audit);
            
            try
            {
                // Reload to get navigation properties
                var result = await _service.GetByIdAsync(created.AuditId);
                if (result != null)
                {
                    Response.Headers.Append("Location", $"/api/InventoryAudit/{created.AuditId}");
                    return StatusCode(201, MapToDto(result, false));
                }
            }
            catch
            {
                // If reload fails, continue to fallback
            }
            
            // Fallback: return basic DTO without navigation properties
            Response.Headers.Append("Location", $"/api/InventoryAudit/{created.AuditId}");
            return StatusCode(201, new
            {
                auditId = created.AuditId,
                auditCode = created.AuditCode,
                warehouseId = created.WarehouseId,
                warehouseName = (string?)null,
                auditDate = created.AuditDate,
                createdByUserId = created.CreatedByUserId,
                createdByUserName = (string?)null,
                status = created.Status,
                note = created.Note,
                createdAt = created.CreatedAt
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Đã xảy ra lỗi khi tạo phiếu kiểm kê", error = ex.Message });
        }
    }

    [HttpPost("{id}/generate-details")]
    [SwaggerOperation(Summary = "Tự động tạo chi tiết kiểm kê từ tồn kho")]
    public async Task<IActionResult> GenerateDetailsAsync(Guid id, [FromQuery] Guid warehouseId)
    {
        var audit = await _service.GetByIdAsync(id);
        if (audit == null)
            return NotFound(new { message = "Không tìm thấy phiếu kiểm kê" });

        if (audit.Status != "InProgress")
            return BadRequest(new { message = "Chỉ có thể tạo chi tiết cho phiếu đang kiểm kê" });

        var details = await _service.GenerateAuditDetailsFromStockAsync(id, warehouseId);
        var result = details.Select(d => MapDetailToDto(d));
        
        return Ok(new
        {
            message = $"Đã tạo {details.Count} chi tiết kiểm kê",
            details = result
        });
    }

    [HttpPost("{id}/details")]
    [SwaggerOperation(Summary = "Thêm/Cập nhật chi tiết kiểm kê")]
    public async Task<IActionResult> AddDetailAsync(Guid id, [FromBody] CreateInventoryAuditDetailDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var audit = await _service.GetByIdAsync(id);
        if (audit == null)
            return NotFound(new { message = "Không tìm thấy phiếu kiểm kê" });

        if (audit.Status != "InProgress")
            return BadRequest(new { message = "Chỉ có thể thêm chi tiết cho phiếu đang kiểm kê" });

        var detail = new InventoryAuditDetail
        {
            AuditDetailId = Guid.NewGuid(),
            AuditId = id, // Use path parameter instead of dto.AuditId
            InventoryAudit = null!,
            ProductId = dto.ProductId,
            Product = null!,
            SystemQuantity = dto.SystemQuantity,
            ActualQuantity = dto.ActualQuantity,
            Variance = 0, // Will be calculated in service
            Note = dto.Note,
            AuditedByUserId = dto.AuditedByUserId
        };

        var created = await _service.AddAuditDetailAsync(detail);
        return Ok(MapDetailToDto(created));
    }

    [HttpPost("{id}/complete")]
    [SwaggerOperation(Summary = "Hoàn thành kiểm kê")]
    public async Task<IActionResult> CompleteAuditAsync(Guid id, [FromQuery] bool updateStock = true)
    {
        try
        {
            var audit = await _service.CompleteAuditAsync(id, updateStock);
            return Ok(new
            {
                message = "Đã hoàn thành kiểm kê",
                audit = MapToDto(audit, false),
                stockUpdated = updateStock
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [SwaggerOperation(Summary = "Hủy kiểm kê")]
    public async Task<IActionResult> CancelAuditAsync(Guid id)
    {
        try
        {
            await _service.CancelAuditAsync(id);
            return Ok(new { message = "Đã hủy phiếu kiểm kê" });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // Helper methods
    private InventoryAuditDto MapToDto(InventoryAudit audit, bool includeStats)
    {
        var dto = new InventoryAuditDto
        {
            AuditId = audit.AuditId,
            AuditCode = audit.AuditCode,
            WarehouseId = audit.WarehouseId,
            WarehouseName = audit.Warehouse?.WarehouseName,
            AuditDate = audit.AuditDate,
            CompletedDate = audit.CompletedDate,
            CreatedByUserId = audit.CreatedByUserId,
            CreatedByUserName = audit.CreatedByUser?.UserName,
            Status = audit.Status,
            Note = audit.Note,
            CreatedAt = audit.CreatedAt
        };

        if (includeStats && audit.InventoryAuditDetails != null && audit.InventoryAuditDetails.Any())
        {
            dto.TotalProducts = audit.InventoryAuditDetails.Count;
            dto.ProductsWithVariance = audit.InventoryAuditDetails.Count(d => d.Variance != 0);
            dto.TotalVariance = audit.InventoryAuditDetails.Sum(d => d.Variance);
        }

        return dto;
    }

    private InventoryAuditDetailDto MapDetailToDto(InventoryAuditDetail detail)
    {
        var variancePercentage = detail.SystemQuantity > 0
            ? (decimal)Math.Abs(detail.Variance) / detail.SystemQuantity * 100
            : 0;

        string varianceStatus = detail.Variance == 0 ? "Match" :
                               detail.Variance < 0 ? "Shortage" : "Excess";

        return new InventoryAuditDetailDto
        {
            AuditDetailId = detail.AuditDetailId,
            AuditId = detail.AuditId,
            ProductId = detail.ProductId,
            ProductName = detail.Product?.ProductName,
            SystemQuantity = detail.SystemQuantity,
            ActualQuantity = detail.ActualQuantity,
            Variance = detail.Variance,
            Note = detail.Note,
            AuditedAt = detail.AuditedAt,
            AuditedByUserId = detail.AuditedByUserId,
            AuditedByUserName = detail.AuditedByUser?.UserName,
            VariancePercentage = variancePercentage,
            VarianceStatus = varianceStatus
        };
    }
}
