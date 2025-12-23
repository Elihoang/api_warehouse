using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class InventoryAuditDetailRepository : BaseRepository<InventoryAuditDetail>, IInventoryAuditDetailRepository
{
    private readonly AppDbContext _context;

    public InventoryAuditDetailRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public async Task<IEnumerable<InventoryAuditDetail>> GetDetailsByAuditIdAsync(Guid auditId)
    {
        return await _context.InventoryAuditDetails
            .Include(d => d.Product)
            .Include(d => d.AuditedByUser)
            .Where(d => d.AuditId == auditId)
            .ToListAsync();
    }

    public async Task<IEnumerable<InventoryAuditDetail>> GetDetailsWithVarianceAsync(Guid auditId)
    {
        return await _context.InventoryAuditDetails
            .Include(d => d.Product)
            .Include(d => d.AuditedByUser)
            .Where(d => d.AuditId == auditId && d.Variance != 0)
            .OrderByDescending(d => Math.Abs(d.Variance))
            .ToListAsync();
    }
}
