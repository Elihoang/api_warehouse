namespace BeWarehouseHub.Share.DTOs.Import;

public class ImportDetailDto
{
    public Guid ImportDetailId { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }        // giá nhập của lô này
    public decimal Amount => Quantity * Price;
    public DateTime DateImport { get; set; }
}