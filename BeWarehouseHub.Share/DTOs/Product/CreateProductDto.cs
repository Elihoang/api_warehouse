using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Product;

public class CreateProductDto
{
    [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
    [MaxLength(150)]
    public string ProductName { get; set; } = string.Empty;

    public Guid? CategoryId { get; set; }

    public Guid? SupplierId { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; } = string.Empty;

    [Range(0, 999999999, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
    public decimal Price { get; set; }
}