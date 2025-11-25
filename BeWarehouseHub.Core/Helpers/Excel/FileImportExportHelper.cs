// File: BeWarehouseHub.Core/Helpers/FileImportExportHelper.cs

using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Share.DTOs.Export;
using BeWarehouseHub.Share.DTOs.Import;
using Microsoft.EntityFrameworkCore;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace BeWarehouseHub.Core.Helpers;

public interface IFileImportExportHelper
{
    Task<ImportExportResult> ImportExportReceiptFromExcelAsync(
        Stream stream, string fileName, Guid warehouseId, Guid userId, CancellationToken ct = default);

    Task<ImportExportResult> ImportImportReceiptFromExcelAsync(
        Stream stream, string fileName, Guid warehouseId, Guid userId, CancellationToken ct = default);
}

public class FileImportExportHelper : IFileImportExportHelper
{
    private readonly AppDbContext _context;
    public FileImportExportHelper(AppDbContext context) => _context = context;

    // ====================== XUẤT KHO ======================
    public async Task<ImportExportResult> ImportExportReceiptFromExcelAsync(
        Stream stream, string fileName, Guid warehouseId, Guid userId, CancellationToken ct = default)
    {
        var details = new List<CreateExportDetailDto>();
        var errors = new List<string>();

        var productDict = await BuildProductLookupDict(ct);

        var readResult = await ReadExcelCoreAsync(stream, fileName, expectedColumns: 2, errors, ct);
        if (!readResult.Success) return readResult.Result!;

        for (int i = 0; i < readResult.Rows.Count; i++)
        {
            var cells = readResult.Rows[i];
            var codeOrName = cells[0]?.Trim();
            if (string.IsNullOrWhiteSpace(codeOrName)) continue;

            if (!int.TryParse(cells[1], out int qty) || qty <= 0)
            {
                errors.Add($"Dòng {i + 2}: Số lượng không hợp lệ");
                continue;
            }

            var productId = await LookupProductId(productDict, codeOrName, i + 2, errors);
            if (!productId.HasValue) continue;

            details.Add(new CreateExportDetailDto { ProductId = productId.Value, Quantity = qty });
        }

        return BuildResult(details, errors);
    }

    // ====================== NHẬP KHO ======================
    public async Task<ImportExportResult> ImportImportReceiptFromExcelAsync(
        Stream stream, string fileName, Guid warehouseId, Guid userId, CancellationToken ct = default)
    {
        var details = new List<CreateImportDetailDto>();
        var errors = new List<string>();

        var productDict = await BuildProductLookupDict(ct);

        var readResult = await ReadExcelCoreAsync(stream, fileName, expectedColumns: 3, errors, ct);
        if (!readResult.Success) return readResult.Result!;

        for (int i = 0; i < readResult.Rows.Count; i++)
        {
            var cells = readResult.Rows[i];
            var codeOrName = cells[0]?.Trim();
            if (string.IsNullOrWhiteSpace(codeOrName))
            {
                errors.Add($"Dòng {i + 2}: Thiếu mã/tên sản phẩm");
                continue;
            }

            if (!int.TryParse(cells[1], out int qty) || qty <= 0)
            {
                errors.Add($"Dòng {i + 2}: Số lượng không hợp lệ");
                continue;
            }

            if (!decimal.TryParse(cells[2], out decimal price) || price < 0)
            {
                errors.Add($"Dòng {i + 2}: Đơn giá nhập không hợp lệ");
                continue;
            }

            var productId = await LookupProductId(productDict, codeOrName, i + 2, errors);
            if (!productId.HasValue) continue;

            details.Add(new CreateImportDetailDto
            {
                ProductId = productId.Value,
                Quantity = qty,
                Price = price
            });
        }

        var result = BuildResult(details, errors);
        result.TempImportDetails = details; // lưu lại để Service lấy
        return result;
    }

    // ==================================================================
    // Private helpers
    // ==================================================================

    private async Task<Dictionary<string, Guid>> BuildProductLookupDict(CancellationToken ct)
    {
        return await _context.Products
            .AsNoTracking()
            .Where(p => !string.IsNullOrWhiteSpace(p.ProductName))
            .ToDictionaryAsync(
                p => p.ProductName.Trim().ToUpperInvariant(),
                p => p.ProductId,
                ct);
    }

    private record ExcelReadResult(bool Success, ImportExportResult? Result, List<string[]> Rows);

    private async Task<ExcelReadResult> ReadExcelCoreAsync(
        Stream stream, string fileName, int expectedColumns, List<string> errors, CancellationToken ct)
    {
        var ext = Path.GetExtension(fileName)?.ToLowerInvariant();
        if (ext is not (".xlsx" or ".xls"))
            return new(false, new() { Success = false, Message = "Chỉ hỗ trợ file .xlsx hoặc .xls" }, null!);

        stream.Position = 0;
        IWorkbook workbook = ext == ".xlsx" ? new XSSFWorkbook(stream) : new HSSFWorkbook(stream);
        var sheet = workbook.GetSheetAt(0);

        if (sheet?.PhysicalNumberOfRows < 2)
            return new(false, new() { Success = false, Message = "File không có dữ liệu" }, null!);

        var rows = new List<string[]>();
        for (int i = 1; i <= sheet.LastRowNum; i++)
        {
            var row = sheet.GetRow(i);
            if (row == null) continue;

            var cells = new string[expectedColumns];
            for (int c = 0; c < expectedColumns; c++)
                cells[c] = row.GetCell(c)?.ToString()?.Trim() ?? "";

            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(cells);
        }

        return new(true, null, rows);
    }

    private async Task<Guid?> LookupProductId(Dictionary<string, Guid> dict, string key, int row, List<string> errors)
    {
        if (dict.TryGetValue(key.ToUpperInvariant(), out var id))
            return id;

        errors.Add($"Dòng {row}: Không tìm thấy sản phẩm '{key}'");
        return null;
    }

    // Overload riêng – rõ ràng, không lỗi kiểu
    private ImportExportResult BuildResult(List<CreateExportDetailDto> details, List<string> errors)
        => BuildResultCore(details.Any() ? details : null, null, errors);

    private ImportExportResult BuildResult(List<CreateImportDetailDto> details, List<string> errors)
        => BuildResultCore(null, details.Any() ? details : null, errors);

    private ImportExportResult BuildResultCore(
        List<CreateExportDetailDto>? exportDetails,
        List<CreateImportDetailDto>? importDetails,
        List<string> errors)
    {
        if (errors.Any())
            return new() { Success = false, Message = "Dữ liệu có lỗi", Errors = errors };

        if ((exportDetails?.Any() != true) && (importDetails?.Any() != true))
            return new() { Success = false, Message = "Không có sản phẩm hợp lệ" };

        return new()
        {
            Success = true,
            Message = "Đọc file thành công",
            TempDetails = exportDetails,
            TempImportDetails = importDetails
        };
    }
}