using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class ProductBatchRepository : BaseRepository<ProductBatch>, IProductBatchRepository
{
    private readonly AppDbContext _context;

    public ProductBatchRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<ProductBatch>> GetAllAsync()
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .ToListAsync();
    }

    public override async Task<ProductBatch?> GetByIdAsync(Guid id)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.BatchId == id);
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByProductIdAsync(Guid productId)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.ProductId == productId)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.WarehouseId == warehouseId)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetExpiringBatchesAsync(int daysUntilExpiry)
    {
        var targetDate = DateTime.UtcNow.AddDays(daysUntilExpiry);
        
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.ExpiryDate <= targetDate && b.Status == "Available")
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<ProductBatch>> GetBatchesByStatusAsync(string status)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .Where(b => b.Status == status)
            .OrderBy(b => b.ExpiryDate)
            .ToListAsync();
    }

    public async Task<ProductBatch?> GetBatchByBatchNumberAsync(string batchNumber)
    {
        return await _context.ProductBatches
            .Include(b => b.Product)
            .Include(b => b.Warehouse)
            .FirstOrDefaultAsync(b => b.BatchNumber == batchNumber);
    }
}
