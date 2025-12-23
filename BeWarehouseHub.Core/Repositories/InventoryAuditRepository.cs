using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class InventoryAuditRepository : BaseRepository<InventoryAudit>, IInventoryAuditRepository
{
    private readonly AppDbContext _context;

    public InventoryAuditRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<InventoryAudit>> GetAllAsync()
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }

    public override async Task<InventoryAudit?> GetByIdAsync(Guid id)
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AuditId == id);
    }

    public async Task<IEnumerable<InventoryAudit>> GetAuditsByWarehouseIdAsync(Guid warehouseId)
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .Where(a => a.WarehouseId == warehouseId)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryAudit>> GetAuditsByStatusAsync(string status)
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.AuditDate)
            .ToListAsync();
    }

    public async Task<InventoryAudit?> GetAuditWithDetailsAsync(Guid auditId)
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .Include(a => a.InventoryAuditDetails)
                .ThenInclude(d => d.Product)
            .FirstOrDefaultAsync(a => a.AuditId == auditId);
    }

    public async Task<InventoryAudit?> GetAuditByCodeAsync(string auditCode)
    {
        return await _context.InventoryAudits
            .Include(a => a.Warehouse)
            .Include(a => a.CreatedByUser)
            .FirstOrDefaultAsync(a => a.AuditCode == auditCode);
    }
}
