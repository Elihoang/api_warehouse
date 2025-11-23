using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Export;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Services;

public class ExportReceiptService
{
    private readonly IExportReceiptRepository _exportReceiptRepository;
    private readonly IStockRepository _stockRepository;
    private readonly AppDbContext _context;

    public ExportReceiptService(
        IExportReceiptRepository exportReceiptRepository,
        IStockRepository stockRepository,
        AppDbContext context)
    {
        _exportReceiptRepository = exportReceiptRepository;
        _stockRepository = stockRepository;
        _context = context;
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
                    throw new InvalidOperationException($"Sản phẩm {item.ProductId} không đủ tồn kho để xuất (yêu cầu: {item.Quantity})");

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
    
}