using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IAutoReorderSettingsRepository : IRepository<AutoReorderSettings>
{
    Task<AutoReorderSettings?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId);
    Task<IEnumerable<AutoReorderSettings>> GetEnabledSettingsAsync();
    Task<IEnumerable<AutoReorderSettings>> GetByWarehouseIdAsync(Guid warehouseId);
}
