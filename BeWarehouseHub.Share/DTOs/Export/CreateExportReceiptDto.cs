using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Export;

public class CreateExportReceiptDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    // Customer Information (optional)
    public string? CustomerName { get; set; }
    public string? CustomerAddress { get; set; }

    [Required]
    public List<CreateExportDetailDto> Details { get; set; } = new();
}