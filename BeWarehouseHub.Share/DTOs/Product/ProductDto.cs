namespace BeWarehouseHub.Share.DTOs.Product;

public class ProductDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public Guid? CategoryId { get; set; }
    public string? SupplierName { get; set; }
    public Guid? SupplierId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime Time { get; set; }
}