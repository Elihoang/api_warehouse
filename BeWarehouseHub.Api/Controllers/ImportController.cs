using BeWarehouseHub.Core.Helpers.Excel;
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Import;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ImportController : ControllerBase
{
    private readonly ImportReceiptService _service;

    public ImportController(ImportReceiptService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả phiếu nhập kho")]
    public async Task<IActionResult> GetAllAsync()
    {
        var receipts = await _service.GetAllAsync();

        var result = receipts.Select(r => new ImportReceiptDto
        {
            ImportId = r.ImportId,
            ImportDate = r.ImportDate,
            WarehouseName = r.Warehouse?.WarehouseName ?? "",
            UserName = r.User.UserName ?? "Không rõ",
            TotalItems = r.ImportDetails?.Count ?? 0,
            TotalQuantity = r.ImportDetails?.Sum(d => d.Quantity) ?? 0,
            TotalAmount = r.ImportDetails?.Sum(d => d.Quantity * d.Price) ?? 0,
            Details = r.ImportDetails?.Select(d => new ImportDetailDto
            {
                ImportDetailId = d.ImportDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                Unit = d.Product?.Unit ?? "",
                Quantity = d.Quantity,
                Price = d.Price,
                DateImport = d.DateImport
            }).ToList() ?? new()
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy chi tiết phiếu nhập theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var receipt = await _service.GetByIdAsync(id);
        if (receipt == null)
            return NotFound(new { message = "Không tìm thấy phiếu nhập" });

        var dto = new ImportReceiptDto
        {
            ImportId = receipt.ImportId,
            ImportDate = receipt.ImportDate,
            WarehouseName = receipt.Warehouse?.WarehouseName ?? "",
            UserName = receipt.User.UserName ?? "",
            TotalItems = receipt.ImportDetails?.Count ?? 0,
            TotalQuantity = receipt.ImportDetails?.Sum(d => d.Quantity) ?? 0,
            TotalAmount = receipt.ImportDetails?.Sum(d => d.Quantity * d.Price) ?? 0,
            Details = receipt.ImportDetails?.Select(d => new ImportDetailDto
            {
                ImportDetailId = d.ImportDetailId,
                ProductId = d.ProductId,
                ProductName = d.Product?.ProductName ?? "",
                Unit = d.Product?.Unit ?? "",
                Quantity = d.Quantity,
                Price = d.Price,
                DateImport = d.DateImport
            }).ToList() ?? new()
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo phiếu nhập kho mới (tự động cộng tồn + lưu giá vốn)")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateImportReceiptDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var receipt = await _service.CreateAsync(dto);
            return Ok(new { message = "Nhập kho thành công", importId = receipt.ImportId });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa phiếu nhập + trừ lại tồn kho (chỉ dùng khi sửa sai)")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Đã xóa phiếu nhập và điều chỉnh tồn kho" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
    [HttpGet("{id}/export-pdf")]
    public async Task<IActionResult> ExportPdf(Guid id)
    {
        var pdf = await _service.GeneratePdfAsync(id);
        var fileName = $"PhieuNhap_{id.ToString("N")[..8].ToUpper()}_{DateTime.Today:yyyyMMdd}.pdf";
        return File(pdf, "application/pdf", fileName);
    }

    [HttpGet("{id}/export-excel")]
    public async Task<IActionResult> ExportExcel(Guid id)
    {
        var excel = await _service.ExportToExcelAsync(id);
        var fileName = $"PhieuNhap_{id.ToString("N")[..8].ToUpper()}_{DateTime.Today:yyyyMMdd}.xlsx";
        return File(excel, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
    }
    [HttpPost("import-excel")]
    public async Task<IActionResult> ImportFromExcel(IFormFile file, [FromForm] Guid warehouseId, [FromForm] Guid userId)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Chưa chọn file");

        await using var stream = file.OpenReadStream();
        var result = await _service.ImportFromExcelAsync(stream, file.FileName, warehouseId, userId);

        return result.Success
            ? Ok(new
            {
                result.Message,
                result.ImportId,
                result.TotalItems,
                result.TotalQuantity,
                result.TotalAmount
            })
            : BadRequest(new { result.Message, errors = result.Errors });
    }
}