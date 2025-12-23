namespace BeWarehouseHub.Share.DTOs.InventoryAudit;

public class InventoryAuditDto
{
    public Guid AuditId { get; set; }
    public string AuditCode { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime AuditDate { get; set; }
    public DateTime? CompletedDate { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public string Status { get; set; } = "InProgress";
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Thống kê
    public int TotalProducts { get; set; }
    public int ProductsWithVariance { get; set; }
    public int TotalVariance { get; set; } // Tổng chênh lệch (có thể âm hoặc dương)
}
