using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Core.Helpers;
using BeWarehouseHub.Core.Helpers.Excel;
using BeWarehouseHub.Core.Helpers.Pdf;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Export;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BeWarehouseHub.Core.Services;

public class ExportReceiptService
{
    private readonly IExportReceiptRepository _exportReceiptRepository;
    private readonly IStockRepository _stockRepository;
    private readonly AppDbContext _context;
    private readonly IFileImportExportHelper _fileHelper;

    public ExportReceiptService(
        IExportReceiptRepository exportReceiptRepository,
        IStockRepository stockRepository,
        AppDbContext context,
        IFileImportExportHelper fileHelper) 
    {
        _exportReceiptRepository = exportReceiptRepository;
        _stockRepository = stockRepository;
        _context = context;
        _fileHelper = fileHelper;
    }

    public async Task<IEnumerable<ExportReceipt>> GetAllAsync()
    {
        return await _context.ExportReceipts
            .Include(e => e.Warehouse)
            .Include(e => e.User)
            .Include(e => e.ExportDetails).ThenInclude(d => d.Product)
            .OrderByDescending(e => e.ExportDate)
            .ToListAsync();
    }

    public async Task<ExportReceipt?> GetByIdAsync(Guid id)
    {
        return await _context.ExportReceipts
            .Include(e => e.Warehouse)
            .Include(e => e.User)
            .Include(e => e.ExportDetails).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(e => e.ExportId == id);
    }

    public async Task<ExportReceipt> CreateAsync(CreateExportReceiptDto dto)
    {
        using var transaction = await _context.Database.BeginTransactionAsync(); // Thêm transaction cho an toàn
        try
        {
            var receipt = new ExportReceipt
            {
                ExportId = Guid.NewGuid(),
                ExportDate = dto.ExportDate,
                WarehouseId = dto.WarehouseId,
                UserId = dto.UserId,
                CustomerName = dto.CustomerName,
                CustomerAddress = dto.CustomerAddress,
                ExportDetails = new List<ExportDetail>()
            };

            foreach (var item in dto.Details)
            {
                // Lấy đúng bản ghi Stock trong kho đó, ưu tiên tồn > 0, cũ nhất trước (FIFO)
                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.WarehouseId == dto.WarehouseId
                                              && s.ProductId == item.ProductId
                                              && s.Quantity > 0); // thêm điều kiện này cực kỳ quan trọng!

                if (stock == null || stock.Quantity < item.Quantity)
                    throw new InvalidOperationException(
                        $"Sản phẩm {item.ProductId} không đủ tồn kho để xuất (yêu cầu: {item.Quantity})");

                // Trừ tồn kho trước
                stock.Quantity -= item.Quantity;

                // Thêm chi tiết xuất
                receipt.ExportDetails.Add(new ExportDetail
                {
                    ExportDetailId = Guid.NewGuid(),
                    ExportId = receipt.ExportId,
                    StockId = stock.StockId,
                    ProductId = item.ProductId,
                    Quantity = item.Quantity,
                    DateExport = dto.ExportDate
                });
            }

            // Chỉ add receipt, KHÔNG gọi repository nếu nó tự SaveChanges
            _context.ExportReceipts.Add(receipt);

            // Lưu 1 lần duy nhất (cả receipt + stock update)
            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return receipt;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    // Delete (chỉ cho phép nếu muốn hoàn tác)
    public async Task DeleteAsync(Guid id)
    {
        var receipt = await GetByIdAsync(id)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu xuất");

        // Hoàn tồn kho (nếu muốn)
        foreach (var detail in receipt.ExportDetails)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.WarehouseId == receipt.WarehouseId && s.ProductId == detail.ProductId);

            if (stock != null)
                stock.Quantity += detail.Quantity;
        }

        _exportReceiptRepository.DeleteAsync(receipt);
        await _context.SaveChangesAsync();
    }

    public async Task<ImportExportResult> ImportFromExcelAsync(
        Stream excelStream,
        string fileName, // chỉ để lấy đuôi .xls/.xlsx
        Guid warehouseId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var result = new ImportExportResult();

        if (excelStream == null || excelStream.Length == 0)
            return new() { Success = false, Message = "File rỗng hoặc không hợp lệ" };

        var ext = Path.GetExtension(fileName)?.ToLowerInvariant() ?? "";
        if (ext != ".xlsx" && ext != ".xls")
            return new() { Success = false, Message = "Chỉ hỗ trợ file .xlsx hoặc .xls" };

        var details = new List<CreateExportDetailDto>();
        var errors = new List<string>();

        // Cache sản phẩm theo mã (ưu tiên) hoặc tên
        var productDict = await _context.Products
            .AsNoTracking()
            .ToDictionaryAsync(
                p => p.ProductId.ToString().ToUpper(),
                p => p.ProductId,
                cancellationToken);

        try
        {
            excelStream.Position = 0;
            IWorkbook workbook = ext == ".xlsx"
                ? new XSSFWorkbook(excelStream)
                : new HSSFWorkbook(excelStream);

            var sheet = workbook.GetSheetAt(0);
            if (sheet == null || sheet.PhysicalNumberOfRows < 2)
                return new() { Success = false, Message = "File Excel không có dữ liệu" };

            for (int i = 1; i <= sheet.LastRowNum; i++)
            {
                var row = sheet.GetRow(i);
                if (row == null) continue;

                var codeOrName = row.GetCell(0)?.ToString()?.Trim();
                if (string.IsNullOrWhiteSpace(codeOrName)) continue;

                var qtyCell = row.GetCell(1);
                if (qtyCell == null ||
                    !double.TryParse(qtyCell.ToString(), out double qty) ||
                    qty <= 0 || qty > int.MaxValue)
                {
                    errors.Add($"Dòng {i + 1}: Số lượng không hợp lệ");
                    continue;
                }

                var key = codeOrName.ToUpper();
                if (!productDict.TryGetValue(key, out var productId))
                {
                    errors.Add($"Dòng {i + 1}: Không tìm thấy sản phẩm có mã/tên '{codeOrName}'");
                    continue;
                }

                details.Add(new CreateExportDetailDto
                {
                    ProductId = productId,
                    Quantity = (int)qty
                });
            }
        }
        catch (Exception ex)
        {
            return new() { Success = false, Message = "Lỗi đọc file Excel: " + ex.Message };
        }

        if (errors.Any())
            return new() { Success = false, Message = "Có lỗi trong dữ liệu", Errors = errors };

        if (!details.Any())
            return new() { Success = false, Message = "Không tìm thấy sản phẩm hợp lệ nào" };

        var createDto = new CreateExportReceiptDto
        {
            WarehouseId = warehouseId,
            UserId = userId,
            ExportDate = DateTime.UtcNow,
            Details = details
        };

        try
        {
            var receipt = await CreateAsync(createDto);

            result.Success = true;
            result.ExportId = receipt.ExportId;
            result.Message = $"Nhập phiếu xuất thành công! Mã phiếu: {receipt.ExportId}";
            result.TotalItems = details.Count;
            result.TotalQuantity = details.Sum(d => d.Quantity);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Message = ex.Message;
        }

        return result;
    }


    /// <summary>
    /// Tạo file PDF phiếu xuất kho trực tiếp từ Entity (không cần DTO từ ngoài truyền vào)
    /// </summary>
    public async Task<byte[]> GeneratePdfAsync(Guid exportId)
    {
        var receipt = await GetByIdAsync(exportId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu");
        var dto = MapToDto(receipt);
        return PdfExportHelper.GenerateExportReceiptPdf(dto);
    }
    
    
    public async Task<ImportExportResult> ImportFromExcelAsync(
        Stream stream, string fileName, Guid warehouseId, Guid userId)
    {
        var helperResult = await _fileHelper.ImportExportReceiptFromExcelAsync(
            stream, fileName, warehouseId, userId);

        if (!helperResult.Success) return helperResult;

        var dto = new CreateExportReceiptDto
        {
            WarehouseId = warehouseId,
            UserId = userId,
            ExportDate = DateTime.UtcNow,
            Details = helperResult.TempDetails!
        };

        var receipt = await CreateAsync(dto); // dùng lại transaction cũ

        return new ImportExportResult
        {
            Success = true,
            ExportId = receipt.ExportId,
            Message = $"Nhập thành công phiếu {receipt.ExportId}",
            TotalItems = dto.Details.Count,
            TotalQuantity = dto.Details.Sum(x => x.Quantity)
        };
    }
    public async Task<byte[]> ExportToExcelAsync(Guid exportId)
    {
        var receipt = await GetByIdAsync(exportId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu");

        var dto = MapToDto(receipt); // bạn đã có sẵn hàm map này trong Controller → copy vào đây hoặc để private
        return ExcelExportHelper.ExportReceiptToExcel(dto);
    }
    private ExportReceiptDto MapToDto(ExportReceipt r) => new()
    {
        ExportId = r.ExportId,
        ExportDate = r.ExportDate,
        WarehouseName = r.Warehouse?.WarehouseName ?? "",
        UserName = r.User?.UserName ?? "",
        TotalItems = r.ExportDetails?.Count ?? 0,
        TotalQuantity = r.ExportDetails?.Sum(d => d.Quantity) ?? 0,
        TotalAmount = r.ExportDetails?.Sum(d => d.Quantity * (d.Product?.Price ?? 0)) ?? 0,
        Details = r.ExportDetails?.Select(d => new ExportDetailDto
        {
            ProductId = d.ProductId,
            ProductName = d.Product?.ProductName ?? "",
            Unit = d.Product?.Unit ?? "Cái",
            Quantity = d.Quantity,
            Price = d.Product?.Price ?? 0,
            DateExport = d.DateExport
        }).ToList() ?? new()
    };

    /// <summary>
    /// Tạo HTML từ template cho phiếu xuất kho
    /// </summary>
    public async Task<string> GenerateHtmlAsync(Guid exportId, string wwwrootPath)
    {
        var receipt = await GetByIdAsync(exportId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu xuất");

        var templatePath = Path.Combine(wwwrootPath, "template", "export_receipt_template.html");
        var template = await TemplateHelper.ReadTemplateAsync(templatePath);

        var totalAmount = receipt.ExportDetails?.Sum(d => d.Quantity * (d.Product?.Price ?? 0)) ?? 0;
        var totalQuantity = receipt.ExportDetails?.Sum(d => d.Quantity) ?? 0;

        // Tạo product rows
        var productRows = TemplateHelper.GenerateProductRows(
            receipt.ExportDetails ?? new List<ExportDetail>(),
            (detail, index) =>
            {
                var subtotal = detail.Quantity * (detail.Product?.Price ?? 0);
                return $@"<tr>
                    <td class=""text-center"">{index}</td>
                    <td>{detail.Product?.ProductName ?? ""}</td>
                    <td class=""text-center"">{detail.Product?.Unit ?? "Cái"}</td>
                    <td class=""text-center"">{detail.Quantity}</td>
                    <td class=""text-right"">{TemplateHelper.FormatCurrency(detail.Product?.Price ?? 0)}</td>
                    <td class=""text-right"">{TemplateHelper.FormatCurrency(subtotal)}</td>
                </tr>";
            });

        // Thay thế placeholders
        var replacements = new Dictionary<string, string>
        {
            { "CompanyName", "CÔNG TY QUẢN LÝ KHO" },
            { "TaxCode", "0123456789" },
            { "CompanyAddress", "123 Đường ABC, Quận 1, TP.HCM" },
            { "PhoneNumber", "0909 123 456" },
            { "Email", "contact@warehouse.com" },
            { "Year", receipt.ExportDate.Year.ToString() },
            { "ReceiptNumber", TemplateHelper.GetShortId(receipt.ExportId) },
            { "Day", receipt.ExportDate.Day.ToString("D2") },
            { "Month", receipt.ExportDate.Month.ToString("D2") },
            { "WarehouseName", receipt.Warehouse?.WarehouseName ?? "" },
            { "UserName", receipt.User?.UserName ?? "" },
            { "CustomerName", receipt.CustomerName ?? "..." },
            { "CustomerAddress", receipt.CustomerAddress ?? "..." },
            { "Reason", "Xuất bán hàng" }, // Optional
            { "ProductRows", productRows },
            { "TotalQuantity", totalQuantity.ToString() },
            { "TotalAmount", TemplateHelper.FormatCurrency(totalAmount) },
            { "AmountInWords", TemplateHelper.NumberToWords(totalAmount) }
        };

        return TemplateHelper.ReplacePlaceholders(template, replacements);
    }
}
