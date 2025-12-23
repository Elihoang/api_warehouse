using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.DemandForecast;

public class CreateDemandForecastDto
{
    [Required(ErrorMessage = "ProductId là bắt buộc")]
    public Guid ProductId { get; set; }

    [Required(ErrorMessage = "WarehouseId là bắt buộc")]
    public Guid WarehouseId { get; set; }

    [Required(ErrorMessage = "ForecastPeriod là bắt buộc")]
    public DateOnly ForecastPeriod { get; set; }

    [Required(ErrorMessage = "Algorithm là bắt buộc")]
    [StringLength(50, ErrorMessage = "Algorithm không được vượt quá 50 ký tự")]
    public string Algorithm { get; set; } = "MovingAverage";

    public string? Note { get; set; }
}
