using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Export;

public class CreateExportReceiptDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime ExportDate { get; set; } = DateTime.UtcNow;

    [Required]
    public List<CreateExportDetailDto> Details { get; set; } = new();
}