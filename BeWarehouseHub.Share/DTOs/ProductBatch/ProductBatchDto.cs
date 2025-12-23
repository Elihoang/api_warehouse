namespace BeWarehouseHub.Share.DTOs.ProductBatch;

public class ProductBatchDto
{
    public Guid BatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime ManufactureDate { get; set; }
    public DateTime ExpiryDate { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = "Available";
    public decimal CostPrice { get; set; }
    public string? Note { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Calculated fields
    public int DaysUntilExpiry { get; set; }
    public bool IsExpiring { get; set; } // Sắp hết hạn (< 30 ngày)
    public bool IsExpired { get; set; }
}
