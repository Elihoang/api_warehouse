using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Warehouse;

public class UpdateWarehouseDto
{
    public Guid WarehouseId { get; set; }

    [Required]
    [MaxLength(150)]
    public string WarehouseName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Location { get; set; }
}