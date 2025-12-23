using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IInventoryAuditRepository : IRepository<InventoryAudit>
{
    Task<IEnumerable<InventoryAudit>> GetAuditsByWarehouseIdAsync(Guid warehouseId);
    Task<IEnumerable<InventoryAudit>> GetAuditsByStatusAsync(string status);
    Task<InventoryAudit?> GetAuditWithDetailsAsync(Guid auditId);
    Task<InventoryAudit?> GetAuditByCodeAsync(string auditCode);
}
