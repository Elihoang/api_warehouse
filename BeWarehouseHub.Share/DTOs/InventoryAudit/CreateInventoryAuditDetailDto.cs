using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.InventoryAudit;

public class CreateInventoryAuditDetailDto
{
    [Required]
    public Guid AuditId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng hệ thống không thể âm")]
    public int SystemQuantity { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng thực tế không thể âm")]
    public int ActualQuantity { get; set; }

    public string? Note { get; set; }

    public Guid? AuditedByUserId { get; set; }
}
