namespace BeWarehouseHub.Share.DTOs.Warehouse;

public class WarehouseDto
{
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;

    // Thống kê
    public int ProductCount { get; set; }      // số sản phẩm khác nhau đang có trong kho (có Quantity > 0)
    public int TotalStockQuantity { get; set; } // tổng số lượng tồn (tất cả sản phẩm)
    public int ImportReceiptCount { get; set; }
    public int ExportReceiptCount { get; set; }
}