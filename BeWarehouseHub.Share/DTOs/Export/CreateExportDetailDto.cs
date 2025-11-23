using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Export;

public class CreateExportDetailDto
{
    [Required]
    public Guid ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}