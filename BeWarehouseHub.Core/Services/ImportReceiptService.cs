using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Core.Helpers;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Import;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Services;

public class ImportReceiptService
{
    private readonly IImportReceiptRepository _importReceiptRepository;
    private readonly IStockRepository _stockRepository;
    private readonly AppDbContext _context;
    private readonly IFileImportExportHelper _fileHelper;
    private readonly ProductBatchService _batchService;

    public ImportReceiptService(
        IImportReceiptRepository importReceiptRepository,
        IFileImportExportHelper fileHelper,
        IStockRepository stockRepository,
        AppDbContext context,
        ProductBatchService batchService)
    {
        _importReceiptRepository = importReceiptRepository;
        _stockRepository = stockRepository;
        _context = context;
        _fileHelper = fileHelper;
        _batchService = batchService;
    }

    public async Task<IEnumerable<ImportReceipt>> GetAllAsync()
    {
        return await _context.ImportReceipts
            .Include(i => i.Warehouse)
            .Include(i => i.User)
            .Include(i => i.ImportDetails).ThenInclude(d => d.Product)
            .OrderByDescending(i => i.ImportDate)
            .ToListAsync();
    }

    public async Task<ImportReceipt?> GetByIdAsync(Guid id)
    {
        return await _context.ImportReceipts
            .Include(i => i.Warehouse)
            .Include(i => i.User)
            .Include(i => i.ImportDetails)
                .ThenInclude(d => d.Product)
                    .ThenInclude(p => p.Supplier) 
            .FirstOrDefaultAsync(i => i.ImportId == id);
    }

    public async Task<ImportReceipt> CreateAsync(CreateImportReceiptDto dto)
    {
        // Validate Warehouse exists
        var warehouseExists = await _context.Warehouses.AnyAsync(w => w.WarehouseId == dto.WarehouseId);
        if (!warehouseExists)
            throw new KeyNotFoundException($"Không tìm thấy kho {dto.WarehouseId}");

        // Validate User exists
        var userExists = await _context.Users.AnyAsync(u => u.UserId == dto.UserId);
        if (!userExists)
            throw new KeyNotFoundException($"Không tìm thấy người dùng {dto.UserId}");

        var receipt = new ImportReceipt
        {
            ImportId = Guid.NewGuid(),
            ImportDate = dto.ImportDate,
            WarehouseId = dto.WarehouseId,
            UserId = dto.UserId,
            ImportDetails = new List<ImportDetail>()
        };

        foreach (var item in dto.Details)
        {
            var product = await _context.Products.FindAsync(item.ProductId)
                ?? throw new KeyNotFoundException($"Không tìm thấy sản phẩm {item.ProductId}");

            // 1. TẠO LÔ HÀNG TỰ ĐỘNG (nếu có thông tin batch)
            Guid? batchId = null;
            if (item.HasBatchInfo)
            {
                // Auto-generate batch number nếu không được cung cấp
                var batchNumber = item.BatchNumber ?? 
                    $"LOT-{product.ProductName.Substring(0, Math.Min(3, product.ProductName.Length)).ToUpper()}-{DateTime.Now:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 4).ToUpper()}";

                var batch = new ProductBatch
                {
                    BatchId = Guid.NewGuid(),
                    BatchNumber = batchNumber,
                    ProductId = item.ProductId,
                    Product = null!,
                    WarehouseId = dto.WarehouseId,
                    Warehouse = null!,
                    ManufactureDate = item.ManufactureDate ?? DateTime.Now.AddDays(-7), // Default 7 ngày trước
                    ExpiryDate = item.ExpiryDate ?? DateTime.Now.AddMonths(6), // Default 6 tháng
                    Quantity = item.Quantity,
                    CostPrice = item.Price,
                    Note = item.BatchNote,
                    Status = "Available",
                    CreatedAt = DateTime.UtcNow,
                    ImportDetails = new List<ImportDetail>(),
                    ExportDetails = new List<ExportDetail>()
                };

                await _batchService.AddAsync(batch);
                batchId = batch.BatchId;
            }

            // 2. Tìm hoặc tạo mới bản ghi tồn kho
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.WarehouseId == dto.WarehouseId && s.ProductId == item.ProductId);

            if (stock == null)
            {
                stock = new Stock
                {
                    StockId = Guid.NewGuid(),
                    WarehouseId = dto.WarehouseId,
                    ProductId = item.ProductId,
                    Quantity = 0
                };
                _context.Stocks.Add(stock);
            }

            // 3. Cộng tồn kho
            stock.Quantity += item.Quantity;

            // 4. Tạo Import Detail với BatchId (nếu có)
            receipt.ImportDetails.Add(new ImportDetail
            {
                ImportDetailId = Guid.NewGuid(),
                ProductId = item.ProductId,
                StockId = stock.StockId,
                Quantity = item.Quantity,
                Price = item.Price,
                DateImport = dto.ImportDate,
                BatchId = batchId  // ← Liên kết với Batch (nếu có)
            });
        }

        await _importReceiptRepository.AddAsync(receipt);
        await _context.SaveChangesAsync();

        return receipt;
    }

    // Xóa phiếu nhập + trừ lại tồn kho (dùng khi sửa sai)
    public async Task DeleteAsync(Guid id)
    {
        var receipt = await GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy phiếu nhập");

        foreach (var detail in receipt.ImportDetails)
        {
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.WarehouseId == receipt.WarehouseId && s.ProductId == detail.ProductId);

            if (stock != null)
            {
                stock.Quantity -= detail.Quantity;
                if (stock.Quantity <= 0)
                    _context.Stocks.Remove(stock);
            }
        }

        _importReceiptRepository.DeleteAsync(receipt);
        await _context.SaveChangesAsync();
    }
    
    public async Task<byte[]> ExportToExcelAsync(Guid importId)
    {
        var receipt = await GetByIdAsync(importId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu nhập");

        var dto = MapToDto(receipt);
        return ExcelImportHelper.ExportReceiptToExcel(dto);
    }
    
    public async Task<byte[]> GeneratePdfAsync(Guid importId)
    {
        var receipt = await GetByIdAsync(importId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu");

        var dto = MapToDto(receipt);
        return PdfImportHelper.GenerateImportReceiptPdf(dto);
    }
    
    public async Task<ImportExportResult> ImportFromExcelAsync(Stream stream, string fileName, Guid warehouseId, Guid userId)
    {
        var helperResult = await _fileHelper.ImportImportReceiptFromExcelAsync(stream, fileName, warehouseId, userId);

        if (!helperResult.Success) return helperResult;

        var dto = new CreateImportReceiptDto
        {
            WarehouseId = warehouseId,
            UserId = userId,
            ImportDate = DateTime.UtcNow,
            Details = helperResult.TempImportDetails!
        };

        var receipt = await CreateAsync(dto);

        return new ImportExportResult
        {
            Success = true,
            ImportId = receipt.ImportId,
            Message = "Nhập kho thành công",
            TotalItems = dto.Details.Count,
            TotalQuantity = dto.Details.Sum(x => x.Quantity),
            TotalAmount = dto.Details.Sum(x => x.Quantity * x.Price)
        };
    }

    private ImportReceiptDto MapToDto(ImportReceipt r) => new()
    {
        ImportId = r.ImportId,
        ImportDate = r.ImportDate,
        WarehouseName = r.Warehouse?.WarehouseName ?? "",
        UserName = r.User?.UserName ?? "",
        Details = r.ImportDetails.Select(d => new ImportDetailDto
        {
            ProductId = d.ProductId,
            ProductName = d.Product?.ProductName ?? "",
            Unit = d.Product?.Unit ?? "Cái",
            Quantity = d.Quantity,
            Price = d.Price,
        }).ToList()
    };

    /// <summary>
    /// Tạo HTML từ template cho phiếu nhập kho
    /// </summary>
    public async Task<string> GenerateHtmlAsync(Guid importId, string wwwrootPath)
    {
        var receipt = await GetByIdAsync(importId)
                      ?? throw new KeyNotFoundException("Không tìm thấy phiếu nhập");

        var templatePath = Path.Combine(wwwrootPath, "template", "import_receipt_template.html");
        var template = await TemplateHelper.ReadTemplateAsync(templatePath);

        var totalAmount = receipt.ImportDetails?.Sum(d => d.Quantity * d.Price) ?? 0;
        var totalQuantity = receipt.ImportDetails?.Sum(d => d.Quantity) ?? 0;

        // Tạo product rows
        var productRows = TemplateHelper.GenerateProductRows(
            receipt.ImportDetails ?? new List<ImportDetail>(),
            (detail, index) =>
            {
                var subtotal = detail.Quantity * detail.Price;
                return $@"<tr>
                    <td class=""text-center"">{index}</td>
                    <td>{detail.Product?.ProductName ?? ""}</td>
                    <td class=""text-center"">{detail.Product?.Unit ?? "Cái"}</td>
                    <td class=""text-center"">{detail.Quantity}</td>
                    <td class=""text-right"">{TemplateHelper.FormatCurrency(detail.Price)}</td>
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
            { "Year", receipt.ImportDate.Year.ToString() },
            { "ReceiptNumber", TemplateHelper.GetShortId(receipt.ImportId) },
            { "Day", receipt.ImportDate.Day.ToString("D2") },
            { "Month", receipt.ImportDate.Month.ToString("D2") },
            { "WarehouseName", receipt.Warehouse?.WarehouseName ?? "" },
            { "UserName", receipt.User?.UserName ?? "" },
            { "SupplierName", GetSupplierInfo(receipt).Name }, 
            { "SupplierAddress", GetSupplierInfo(receipt).Address }, 
            { "Notes", "" }, 
            { "ProductRows", productRows },
            { "TotalQuantity", totalQuantity.ToString() },
            { "TotalAmount", TemplateHelper.FormatCurrency(totalAmount) },
            { "AmountInWords", TemplateHelper.NumberToWords(totalAmount) }
        };

        return TemplateHelper.ReplacePlaceholders(template, replacements);
    }

    /// <summary>
    /// Lấy thông tin nhà cung cấp từ các sản phẩm trong phiếu nhập
    /// Nếu có nhiều NCC, lấy NCC đầu tiên
    /// </summary>
    private (string Name, string Address) GetSupplierInfo(ImportReceipt receipt)
    {
        var supplier = receipt.ImportDetails?
            .Select(d => d.Product?.Supplier)
            .FirstOrDefault(s => s != null);

        if (supplier != null)
        {
            return (supplier.SupplierName ?? "Chưa xác định", supplier.Address ?? "");
        }

        return ("Chưa xác định", "");
    }
    
}