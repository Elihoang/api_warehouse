using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Services;

public class WarehouseService
{
    private readonly IWarehouseRepository _warehouseRepository;
    private readonly AppDbContext _context;

    public WarehouseService(IWarehouseRepository warehouseRepository, AppDbContext context)
    {
        _warehouseRepository = warehouseRepository;
        _context = context;
    }

    public async Task<IEnumerable<Warehouse>> GetAllAsync()
    {
        return await _context.Warehouses
            .Include(w => w.Stocks).ThenInclude(s => s.Product)
            .Include(w => w.ImportReceipts)
            .Include(w => w.ExportReceipts)
            .ToListAsync();
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id)
    {
        return await _context.Warehouses
            .Include(w => w.Stocks).ThenInclude(s => s.Product)
            .Include(w => w.ImportReceipts)
            .Include(w => w.ExportReceipts)
            .FirstOrDefaultAsync(w => w.WarehouseId == id);
    }

    public async Task AddAsync(Warehouse warehouse)
        => await _warehouseRepository.AddAsync(warehouse);

    public async Task UpdateAsync(Warehouse warehouse)
        => await _warehouseRepository.UpdateAsync(warehouse);

    public async Task DeleteAsync(Warehouse warehouse)
        => await _warehouseRepository.DeleteAsync(warehouse);
}