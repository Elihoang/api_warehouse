using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class InventoryAudit
{
    [Key]
    public Guid AuditId { get; set; }

    [Required]
    [MaxLength(100)]
    public required string AuditCode { get; set; } // Mã kiểm kê

    public Guid WarehouseId { get; set; }
    public required Warehouse Warehouse { get; set; }

    public DateTime AuditDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public Guid CreatedByUserId { get; set; }
    public required User CreatedByUser { get; set; }

    [MaxLength(50)]
    public string Status { get; set; } = "InProgress"; // InProgress, Completed, Cancelled

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<InventoryAuditDetail> InventoryAuditDetails { get; set; } = new List<InventoryAuditDetail>();
}
