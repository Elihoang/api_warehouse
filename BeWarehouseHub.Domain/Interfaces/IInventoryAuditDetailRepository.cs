using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IInventoryAuditDetailRepository : IRepository<InventoryAuditDetail>
{
    Task<IEnumerable<InventoryAuditDetail>> GetDetailsByAuditIdAsync(Guid auditId);
    Task<IEnumerable<InventoryAuditDetail>> GetDetailsWithVarianceAsync(Guid auditId);
}
