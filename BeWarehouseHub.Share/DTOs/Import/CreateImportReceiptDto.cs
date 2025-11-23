using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Import;

public class CreateImportReceiptDto
{
    [Required]
    public Guid WarehouseId { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public DateTime ImportDate { get; set; } = DateTime.UtcNow;

    [Required]
    public List<CreateImportDetailDto> Details { get; set; } = new();
}