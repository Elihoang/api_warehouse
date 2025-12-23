namespace BeWarehouseHub.Share.DTOs.DemandForecast;

public class DemandForecastDto
{
    public Guid ForecastId { get; set; }
    public Guid ProductId { get; set; }
    public string? ProductName { get; set; }
    public Guid WarehouseId { get; set; }
    public string? WarehouseName { get; set; }
    public DateTime ForecastPeriod { get; set; }
    public int PredictedDemand { get; set; }
    public int ActualDemand { get; set; }
    public decimal Accuracy { get; set; }
    public string Algorithm { get; set; } = "MovingAverage";
    public int RecommendedOrderQuantity { get; set; }
    public DateTime? SuggestedOrderDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? Note { get; set; }
    
    // Calculated fields
    public int CurrentStock { get; set; }
    public bool NeedReorder { get; set; }
    public int DaysUntilReorder { get; set; }
}
