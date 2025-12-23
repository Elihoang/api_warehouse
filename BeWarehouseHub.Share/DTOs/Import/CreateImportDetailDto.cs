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

    // ========== THÔNG TIN LÔ HÀNG (TÙY CHỌN) ==========
    // Nếu có các field này, hệ thống sẽ TẠO LÔ HÀNG TỰ ĐỘNG
    
    [MaxLength(50)]
    public string? BatchNumber { get; set; }  // Mã lô (nếu null sẽ tự gen)
    
    public DateTime? ManufactureDate { get; set; }  // Ngày sản xuất
    
    public DateTime? ExpiryDate { get; set; }  // Hạn sử dụng
    
    public string? BatchNote { get; set; }  // Ghi chú về lô hàng
    
    // Helper property để check có cần tạo batch không
    public bool HasBatchInfo => ExpiryDate.HasValue || !string.IsNullOrEmpty(BatchNumber);
}
