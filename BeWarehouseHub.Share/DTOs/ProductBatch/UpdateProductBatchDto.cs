using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.ProductBatch;

public class UpdateProductBatchDto
{
    [Range(0, int.MaxValue, ErrorMessage = "Số lượng không thể âm")]
    public int? Quantity { get; set; }

    [MaxLength(50)]
    public string? Status { get; set; }

    public string? Note { get; set; }
}
