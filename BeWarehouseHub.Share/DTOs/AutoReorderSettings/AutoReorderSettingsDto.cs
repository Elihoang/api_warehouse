namespace BeWarehouseHub.Share.DTOs.AutoReorderSettings;

public class AutoReorderSettingsDto
{
    public Guid SettingId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public int MinStockLevel { get; set; }
    public int ReorderPoint { get; set; }
    public int ReorderQuantity { get; set; }
    public int MaxStockLevel { get; set; }
    public bool IsAutoReorderEnabled { get; set; }
    public int LeadTimeDays { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Calculated fields
    public int CurrentStock { get; set; }
    public bool ShouldReorder { get; set; }
}
