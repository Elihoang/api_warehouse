using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class AutoReorderSettingsRepository : BaseRepository<AutoReorderSettings>, IAutoReorderSettingsRepository
{
    private readonly AppDbContext _context;

    public AutoReorderSettingsRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<AutoReorderSettings>> GetAllAsync()
    {
        return await _context.AutoReorderSettings
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .ToListAsync();
    }

    public override async Task<AutoReorderSettings?> GetByIdAsync(Guid id)
    {
        return await _context.AutoReorderSettings
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.SettingId == id);
    }

    public async Task<AutoReorderSettings?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId)
    {
        return await _context.AutoReorderSettings
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
    }

    public async Task<IEnumerable<AutoReorderSettings>> GetEnabledSettingsAsync()
    {
        return await _context.AutoReorderSettings
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.IsAutoReorderEnabled)
            .ToListAsync();
    }

    public async Task<IEnumerable<AutoReorderSettings>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.AutoReorderSettings
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == warehouseId)
            .ToListAsync();
    }
}
