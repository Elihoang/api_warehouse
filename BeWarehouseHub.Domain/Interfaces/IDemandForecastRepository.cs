using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IDemandForecastRepository : IRepository<DemandForecast>
{
    Task<IEnumerable<DemandForecast>> GetForecastsByProductIdAsync(Guid productId);
    Task<IEnumerable<DemandForecast>> GetForecastsByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<DemandForecast>> GetForecastsByPeriodAsync(DateTime period);
    Task<DemandForecast?> GetLatestForecastAsync(Guid productId, Guid warehouseId);
}
