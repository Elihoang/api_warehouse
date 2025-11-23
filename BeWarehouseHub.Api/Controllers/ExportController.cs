using BeWarehouseHub.Core.Helpers.Excel;
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Share.DTOs.Export;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ExportController : ControllerBase
{
    private readonly ExportReceiptService _service;

    public ExportController(ExportReceiptService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả phiếu xuất kho")]
    public async Task<IActionResult> GetAllAsync()
    {
        var receipts = await _service.GetAllAsync();

        var result = receipts.Select(r => new ExportReceiptDto
        {
            ExportId = r.ExportId,
            ExportDate = r.ExportDate,
            WarehouseName = r.Warehouse?.WarehouseName ?? "",
            UserName = r.User.UserName ?? "Không rõ",
            TotalItems = r.ExportDetails?.Count ?? 0,
            TotalQuantity = r.ExportDetails?.Sum(d => d.Quantity) ?? 0,
            TotalAmount = r.ExportDetails?.Sum(d => d.Quantity * d.Product.Price) ?? 0,
            Details = r.ExportDetails?.Select(d => new ExportDetailDto
            {
                ExportDetailId = d.ExportDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                Unit = d.Product?.Unit ?? "",
                Quantity = d.Quantity,
                Price = d.Product?.Price ?? 0,
                DateExport = d.DateExport
            }).ToList() ?? new()
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy chi tiết phiếu xuất theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var receipt = await _service.GetByIdAsync(id);
        if (receipt == null)
            return NotFound(new { message = "Không tìm thấy phiếu xuất" });

        var dto = new ExportReceiptDto
        {
            ExportId = receipt.ExportId,
            ExportDate = receipt.ExportDate,
            WarehouseName = receipt.Warehouse?.WarehouseName ?? "",
            UserName = receipt.User.UserName ?? "",
            TotalItems = receipt.ExportDetails?.Count ?? 0,
            TotalQuantity = receipt.ExportDetails?.Sum(d => d.Quantity) ?? 0,
            TotalAmount = receipt.ExportDetails?.Sum(d => d.Quantity * (d.Product?.Price ?? 0)) ?? 0,
            Details = receipt.ExportDetails?.Select(d => new ExportDetailDto
            {
                ExportDetailId = d.ExportDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                Unit = d.Product?.Unit ?? "",
                Quantity = d.Quantity,
                Price = d.Product?.Price ?? 0,
                DateExport = d.DateExport
            }).ToList() ?? new()
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo phiếu xuất kho mới (tự động trừ tồn)")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateExportReceiptDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var receipt = await _service.CreateAsync(dto);
            return Ok(new { message = "Xuất kho thành công", exportId = receipt.ExportId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa phiếu xuất + hoàn tồn kho (chỉ dùng khi cần sửa sai)")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Đã xóa phiếu xuất và hoàn tồn kho" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
   
    [HttpGet("{id}/export-excel")]
    [SwaggerOperation(Summary = "Xuất phiếu xuất kho ra file Excel")]
    [ProducesResponseType(typeof(FileResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExportExportToExcel(Guid id)
    {
        var receipt = await _service.GetByIdAsync(id);
        if (receipt == null)
            return NotFound(new { message = "Không tìm thấy phiếu xuất kho" });

        // Map sang DTO để dùng chung helper
        var exportDto = new ExportReceiptDto
        {
            ExportId = receipt.ExportId,
            ExportDate = receipt.ExportDate,
            WarehouseName = receipt.Warehouse?.WarehouseName ?? "Không xác định",
            UserName = receipt.User?.UserName ?? "Không xác định",
            Details = receipt.ExportDetails.Select(d => new ExportDetailDto
            {
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "Không rõ",
                Unit = d.Product?.Unit ?? "Cái",
                Quantity = d.Quantity,
                Price = d.Product?.Price ?? 0,
                DateExport = d.DateExport
            }).ToList()
        };
        var bytes = ExcelExportHelper.ExportExportReceiptToExcel(exportDto);
        var fileName = $"PhieuXuat_{receipt.ExportDate:yyyyMMdd}_{receipt.ExportId.ToString("N")[..8].ToUpper()}.xlsx";

        // Tên file ngắn gọn nhất có thể
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        // "PhieuXuat.xlsx");   // khỏi đặt tên dài nữa


    }
}