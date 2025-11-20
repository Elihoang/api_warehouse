using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Category;

public class CreateCategoryDto
{
    [Required(ErrorMessage = "Tên danh mục là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục phải từ 2-100 ký tự")]
    public string CategoryName { get; set; } = string.Empty;
}