using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class ProductBatch
{
    [Key]
    public Guid BatchId { get; set; }

    [Required]
    [MaxLength(50)]
    public required string BatchNumber { get; set; } // Mã lô hàng

    public Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public Guid WarehouseId { get; set; }
    public required Warehouse Warehouse { get; set; }

    public DateTime ManufactureDate { get; set; } // Ngày sản xuất

    public DateTime ExpiryDate { get; set; } // Hạn sử dụng

    public int Quantity { get; set; } // Số lượng trong lô

    [MaxLength(50)]
    public string Status { get; set; } = "Available"; // Available, Expired, Recalled, Sold

    [Column(TypeName = "numeric(18,2)")]
    public decimal CostPrice { get; set; } // Giá nhập của lô này

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ICollection<ImportDetail> ImportDetails { get; set; } = new List<ImportDetail>();
    public ICollection<ExportDetail> ExportDetails { get; set; } = new List<ExportDetail>();
}
