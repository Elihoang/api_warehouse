using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Warehouse;

public class CreateWarehouseDto
{
    [Required(ErrorMessage = "Tên kho là bắt buộc")]
    [MaxLength(150)]
    public string WarehouseName { get; set; } = string.Empty;

    [MaxLength(255)]
    public string? Location { get; set; }
}