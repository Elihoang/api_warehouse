
using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class StockRepository : BaseRepository<Stock>, IStockRepository
{
    public StockRepository(AppDbContext context) : base(context)
    {
    }
    
    public override async Task<IEnumerable<Stock>> GetAllAsync()
    {
        return await _context.Stocks
            .Include(s => s.Warehouse)
            .Include(s => s.Product)
            .ToListAsync();
    }

    public async Task<Stock?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.WarehouseId == warehouseId);
    }

    public async Task<IEnumerable<Stock>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.Stocks
            .Include(s => s.Product)
            .Include(s => s.Warehouse)
            .Where(s => s.WarehouseId == warehouseId)
            .ToListAsync();
    }
}