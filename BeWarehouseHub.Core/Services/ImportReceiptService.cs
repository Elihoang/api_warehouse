using BeWarehouseHub.Core.Configurations;
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

    public ImportReceiptService(
        IImportReceiptRepository importReceiptRepository,
        IStockRepository stockRepository,
        AppDbContext context)
    {
        _importReceiptRepository = importReceiptRepository;
        _stockRepository = stockRepository;
        _context = context;
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
            .Include(i => i.ImportDetails).ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(i => i.ImportId == id);
    }

    public async Task<ImportReceipt> CreateAsync(CreateImportReceiptDto dto)
    {
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

            // Tìm hoặc tạo mới bản ghi tồn kho
            var stock = await _context.Stocks
                .FirstOrDefaultAsync(s => s.WarehouseId == dto.WarehouseId && s.ProductId == item.ProductId);

            if (stock == null)
            {
                stock = new Stock
                {
                    WarehouseId = dto.WarehouseId,
                    ProductId = item.ProductId,
                    Quantity = 0
                };
                _context.Stocks.Add(stock);
            }

            // Cộng tồn kho
            stock.Quantity += item.Quantity;

            receipt.ImportDetails.Add(new ImportDetail
            {
                ImportDetailId = Guid.NewGuid(),
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                Price = item.Price,
                DateImport = dto.ImportDate
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
}