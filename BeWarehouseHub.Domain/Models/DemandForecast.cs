using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class DemandForecast
{
    [Key]
    public Guid ForecastId { get; set; }

    public Guid ProductId { get; set; }
    public required Product Product { get; set; }

    public Guid WarehouseId { get; set; }
    public required Warehouse Warehouse { get; set; }

    public DateTime ForecastPeriod { get; set; } // Kỳ dự báo (tháng)

    public int PredictedDemand { get; set; } // Nhu cầu dự đoán

    public int ActualDemand { get; set; } = 0; // Nhu cầu thực tế (cập nhật sau)

    [Column(TypeName = "numeric(5,2)")]
    public decimal Accuracy { get; set; } = 0; // Độ chính xác (%)

    [MaxLength(50)]
    public string Algorithm { get; set; } = "MovingAverage"; // MovingAverage, LinearRegression, ExponentialSmoothing

    public int RecommendedOrderQuantity { get; set; } // Số lượng đề xuất đặt hàng

    public DateTime? SuggestedOrderDate { get; set; } // Ngày đề xuất đặt hàng

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Note { get; set; }
}
