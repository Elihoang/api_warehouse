using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class ProductBatchService
{
    private readonly IProductBatchRepository _batchRepository;
    private readonly IStockRepository _stockRepository;

    public ProductBatchService(
        IProductBatchRepository batchRepository,
        IStockRepository stockRepository)
    {
        _batchRepository = batchRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<ProductBatch>> GetAllAsync()
        => await _batchRepository.GetAllAsync();

    public async Task<ProductBatch?> GetByIdAsync(Guid id)
        => await _batchRepository.GetByIdAsync(id);

    public async Task<ProductBatch?> GetByBatchNumberAsync(string batchNumber)
        => await _batchRepository.GetBatchByBatchNumberAsync(batchNumber);

    public async Task<IEnumerable<ProductBatch>> GetBatchesByProductIdAsync(Guid productId)
        => await _batchRepository.GetBatchesByProductIdAsync(productId);

    public async Task<IEnumerable<ProductBatch>> GetBatchesByWarehouseIdAsync(Guid warehouseId)
        => await _batchRepository.GetBatchesByWarehouseIdAsync(warehouseId);

    public async Task<IEnumerable<ProductBatch>> GetExpiringBatchesAsync(int daysUntilExpiry = 30)
        => await _batchRepository.GetExpiringBatchesAsync(daysUntilExpiry);

    public async Task<IEnumerable<ProductBatch>> GetBatchesByStatusAsync(string status)
        => await _batchRepository.GetBatchesByStatusAsync(status);

    public async Task AddAsync(ProductBatch batch)
    {
        // Kiểm tra batch number đã tồn tại chưa
        var existing = await _batchRepository.GetBatchByBatchNumberAsync(batch.BatchNumber);
        if (existing != null)
        {
            throw new InvalidOperationException($"Mã lô {batch.BatchNumber} đã tồn tại.");
        }

        // Kiểm tra ngày hết hạn phải sau ngày sản xuất
        if (batch.ExpiryDate <= batch.ManufactureDate)
        {
            throw new InvalidOperationException("Ngày hết hạn phải sau ngày sản xuất.");
        }

        // Convert DateTime to UTC for PostgreSQL
        batch.ManufactureDate = DateTime.SpecifyKind(batch.ManufactureDate, DateTimeKind.Utc);
        batch.ExpiryDate = DateTime.SpecifyKind(batch.ExpiryDate, DateTimeKind.Utc);
        batch.CreatedAt = DateTime.UtcNow;

        await _batchRepository.AddAsync(batch);
    }

    public async Task UpdateAsync(ProductBatch batch)
        => await _batchRepository.UpdateAsync(batch);

    public async Task DeleteAsync(ProductBatch batch)
        => await _batchRepository.DeleteAsync(batch);

    /// <summary>
    /// Tự động cập nhật status của các batch đã hết hạn
    /// </summary>
    public async Task UpdateExpiredBatchesAsync()
    {
        var activeBatches = await _batchRepository.GetBatchesByStatusAsync("Available");
        var now = DateTime.UtcNow;

        foreach (var batch in activeBatches)
        {
            if (batch.ExpiryDate < now)
            {
                batch.Status = "Expired";
                await _batchRepository.UpdateAsync(batch);
            }
        }
    }

    /// <summary>
    /// Lấy danh sách batch theo FIFO (First In First Out) - hết hạn sớm nhất trước
    /// </summary>
    public async Task<IEnumerable<ProductBatch>> GetAvailableBatchesFIFOAsync(Guid productId, Guid warehouseId)
    {
        var allBatches = await _batchRepository.GetBatchesByProductIdAsync(productId);
        return allBatches
            .Where(b => b.WarehouseId == warehouseId && b.Status == "Available" && b.Quantity > 0)
            .OrderBy(b => b.ExpiryDate)
            .ToList();
    }
}
