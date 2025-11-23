namespace BeWarehouseHub.Share.DTOs.Export;

public class ExportDetailDto
{
    public Guid ExportDetailId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }          // giá xuất tại thời điểm
    public decimal Amount => Quantity * Price;
    public DateTime DateExport { get; set; }
}