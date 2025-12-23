using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class AutoReorderSettings
{
    [Key]
    public Guid SettingId { get; set; }

    public Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public Guid WarehouseId { get; set; }
    public required Warehouse Warehouse { get; set; }

    public int MinStockLevel { get; set; } // Mức tồn kho tối thiểu

    public int ReorderPoint { get; set; } // Điểm đặt hàng lại

    public int ReorderQuantity { get; set; } // Số lượng đặt hàng mỗi lần

    public int MaxStockLevel { get; set; } // Mức tồn kho tối đa

    public bool IsAutoReorderEnabled { get; set; } = true; // Bật/tắt tự động đặt hàng

    public int LeadTimeDays { get; set; } = 7; // Thời gian giao hàng (ngày)

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}
