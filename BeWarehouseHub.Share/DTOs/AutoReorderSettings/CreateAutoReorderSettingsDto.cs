using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.AutoReorderSettings;

public class CreateAutoReorderSettingsDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    [Range(0, int.MaxValue, ErrorMessage = "Mức tồn tối thiểu không thể âm")]
    public int MinStockLevel { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Điểm đặt hàng lại phải lớn hơn 0")]
    public int ReorderPoint { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Số lượng đặt hàng phải lớn hơn 0")]
    public int ReorderQuantity { get; set; }

    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "Mức tồn tối đa phải lớn hơn 0")]
    public int MaxStockLevel { get; set; }

    public bool IsAutoReorderEnabled { get; set; } = true;

    [Range(1, 365, ErrorMessage = "Thời gian giao hàng phải từ 1-365 ngày")]
    public int LeadTimeDays { get; set; } = 7;
}
