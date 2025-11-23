namespace BeWarehouseHub.Share.DTOs.Export;

public class ExportReceiptDto
{
    public Guid ExportId { get; set; }
    public DateTime ExportDate { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int TotalItems { get; set; }         // tổng số dòng chi tiết
    public int TotalQuantity { get; set; }      // tổng số lượng xuất
    public decimal TotalAmount { get; set; }    // tổng tiền (nếu cần)
    public List<ExportDetailDto> Details { get; set; } = new();
}