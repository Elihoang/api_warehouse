namespace BeWarehouseHub.Share.DTOs.Import;

public class ImportReceiptDto
{
    public Guid ImportId { get; set; }
    public DateTime ImportDate { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    
    public int TotalItems { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalAmount { get; set; }  // tổng tiền nhập = sum(Quantity * Price)
    
    public List<ImportDetailDto> Details { get; set; } = new();
}