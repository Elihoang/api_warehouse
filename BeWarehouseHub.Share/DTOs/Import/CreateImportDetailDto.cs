using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Import;

public class CreateImportDetailDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Range(0, 999999999)]
    public decimal Price { get; set; }  // giá nhập của lô này
}
