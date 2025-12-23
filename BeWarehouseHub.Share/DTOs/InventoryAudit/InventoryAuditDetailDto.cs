namespace BeWarehouseHub.Share.DTOs.InventoryAudit;

public class InventoryAuditDetailDto
{
    public Guid AuditDetailId { get; set; }
    public Guid AuditId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public int SystemQuantity { get; set; }
    public int ActualQuantity { get; set; }
    public int Variance { get; set; }
    public string? Note { get; set; }
    public DateTime AuditedAt { get; set; }
    public Guid? AuditedByUserId { get; set; }
    public string? AuditedByUserName { get; set; }
    
    // Calculated fields
    public decimal VariancePercentage { get; set; }
    public string VarianceStatus { get; set; } = "Match"; // Match, Shortage, Excess
}
