using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.InventoryAudit;

public class CreateInventoryAuditDto
{
    [Required]
    [MaxLength(100)]
    public string AuditCode { get; set; } = string.Empty;

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public DateTime AuditDate { get; set; }

    [Required]
    public Guid CreatedByUserId { get; set; }

    public string? Note { get; set; }
}
