using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class DemandForecastRepository : BaseRepository<DemandForecast>, IDemandForecastRepository
{
    private readonly AppDbContext _context;

    public DemandForecastRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<DemandForecast>> GetAllAsync()
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .OrderByDescending(f => f.ForecastPeriod)
            .ToListAsync();
    }

    public override async Task<DemandForecast?> GetByIdAsync(Guid id)
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .FirstOrDefaultAsync(f => f.ForecastId == id);
    }

    public async Task<IEnumerable<DemandForecast>> GetForecastsByProductIdAsync(Guid productId)
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .Where(f => f.ProductId == productId)
            .OrderByDescending(f => f.ForecastPeriod)
            .ToListAsync();
    }

    public async Task<IEnumerable<DemandForecast>> GetForecastsByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .Where(f => f.WarehouseId == warehouseId)
            .OrderByDescending(f => f.ForecastPeriod)
            .ToListAsync();
    }

    public async Task<IEnumerable<DemandForecast>> GetForecastsByPeriodAsync(DateTime period)
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .Where(f => f.ForecastPeriod.Year == period.Year && f.ForecastPeriod.Month == period.Month)
            .ToListAsync();
    }

    public async Task<DemandForecast?> GetLatestForecastAsync(Guid productId, Guid warehouseId)
    {
        return await _context.DemandForecasts
            .Include(f => f.Product)
            .Include(f => f.Warehouse)
            .Where(f => f.ProductId == productId && f.WarehouseId == warehouseId)
            .OrderByDescending(f => f.ForecastPeriod)
            .FirstOrDefaultAsync();
    }
}
