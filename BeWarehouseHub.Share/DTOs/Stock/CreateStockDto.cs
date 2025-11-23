using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Stock;

public class CreateStockDto
{
    
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid ProductId { get; set; }
    
    public Guid StockId { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Số lượng không được âm")]
    public int Quantity { get; set; } = 0;
}