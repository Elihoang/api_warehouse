using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Stock;

public class UpdateStockDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid ProductId { get; set; }

    [Range(0, int.MaxValue)]
    public int Quantity { get; set; }
}