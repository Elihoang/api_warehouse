namespace BeWarehouseHub.Share.DTOs.Category;

public class CategoryDto
{
    public Guid CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    
    public int ProductCount { get; set; }
}