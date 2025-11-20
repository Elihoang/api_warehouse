using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Category;

public class UpdateCategoryDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, MinimumLength = 2)]
    public string CategoryName { get; set; } = string.Empty;
}