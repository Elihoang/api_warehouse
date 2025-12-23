using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IStockRepository : IRepository<Stock>
{
    Task<Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<Stock>> GetByWarehouseIdAsync(Guid warehouseId);
}