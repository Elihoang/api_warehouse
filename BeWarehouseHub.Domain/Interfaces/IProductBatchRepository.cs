using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IProductBatchRepository : IRepository<ProductBatch>
{
    Task<IEnumerable<ProductBatch>> GetBatchesByProductIdAsync(Guid productId);
    Task<IEnumerable<ProductBatch>> GetBatchesByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<ProductBatch>> GetExpiringBatchesAsync(int daysUntilExpiry);
    Task<IEnumerable<ProductBatch>> GetBatchesByStatusAsync(string status);
    Task<ProductBatch?> GetBatchByBatchNumberAsync(string batchNumber);
}
