using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class InventoryAuditDetail
{
    [Key]
    public Guid AuditDetailId { get; set; }

    public Guid AuditId { get; set; }
    [ForeignKey("AuditId")]
    public required virtual InventoryAudit InventoryAudit { get; set; }

    public Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public int SystemQuantity { get; set; } // Số lượng trên hệ thống

    public int ActualQuantity { get; set; } // Số lượng kiểm kê thực tế

    public int Variance { get; set; } // Chênh lệch (ActualQuantity - SystemQuantity)

    [MaxLength(500)]
    public string? Note { get; set; }

    public DateTime AuditedAt { get; set; } = DateTime.UtcNow;

    public Guid? AuditedByUserId { get; set; }
    public User? AuditedByUser { get; set; }
}
