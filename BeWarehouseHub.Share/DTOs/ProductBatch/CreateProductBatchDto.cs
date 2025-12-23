using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.ProductBatch;

public class CreateProductBatchDto
{
    [Required]
    [MaxLength(50)]
    public string BatchNumber { get; set; } = string.Empty;

    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public DateTime ManufactureDate { get; set; }

    [Required]
    public DateTime ExpiryDate { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
    public int Quantity { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Giá nhập phải lớn hơn 0")]
    public decimal CostPrice { get; set; }

    public string? Note { get; set; }
}
