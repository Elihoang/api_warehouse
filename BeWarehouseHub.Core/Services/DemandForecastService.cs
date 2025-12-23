using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class DemandForecastService
{
    private readonly IDemandForecastRepository _forecastRepository;
    private readonly IExportDetailRepository _exportDetailRepository;
    private readonly IStockRepository _stockRepository;
    private readonly IAutoReorderSettingsRepository _reorderSettingsRepository;

    public DemandForecastService(
        IDemandForecastRepository forecastRepository,
        IExportDetailRepository exportDetailRepository,
        IStockRepository stockRepository,
        IAutoReorderSettingsRepository reorderSettingsRepository)
    {
        _forecastRepository = forecastRepository;
        _exportDetailRepository = exportDetailRepository;
        _stockRepository = stockRepository;
        _reorderSettingsRepository = reorderSettingsRepository;
    }

    public async Task<IEnumerable<DemandForecast>> GetAllAsync()
        => await _forecastRepository.GetAllAsync();

    public async Task<DemandForecast?> GetByIdAsync(Guid id)
        => await _forecastRepository.GetByIdAsync(id);

    public async Task<IEnumerable<DemandForecast>> GetForecastsByProductIdAsync(Guid productId)
        => await _forecastRepository.GetForecastsByProductIdAsync(productId);

    public async Task<IEnumerable<DemandForecast>> GetForecastsByWarehouseIdAsync(Guid warehouseId)
        => await _forecastRepository.GetForecastsByWarehouseIdAsync(warehouseId);

    public async Task<DemandForecast?> GetLatestForecastAsync(Guid productId, Guid warehouseId)
        => await _forecastRepository.GetLatestForecastAsync(productId, warehouseId);

    /// <summary>
    /// Tạo dự báo nhu cầu cho tháng tiếp theo
    /// </summary>
    public async Task<DemandForecast> GenerateForecastAsync(
        Guid productId,
        Guid warehouseId,
        DateTime forecastPeriod,
        string algorithm = "MovingAverage",
        int lookbackMonths = 3)
    {
        // Lấy dữ liệu xuất kho trong N tháng qua
        var historicalData = await GetHistoricalExportDataAsync(productId, warehouseId, lookbackMonths);

        int predictedDemand = algorithm.ToLower() switch
        {
            "movingaverage" => CalculateMovingAverage(historicalData),
            "weightedmovingaverage" => CalculateWeightedMovingAverage(historicalData),
            "exponentialsmoothing" => CalculateExponentialSmoothing(historicalData),
            _ => CalculateMovingAverage(historicalData)
        };

        // Lấy thông tin reorder settings (nếu có)
        var reorderSettings = await _reorderSettingsRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
        
        // Tính số lượng đề xuất đặt hàng
        var currentStock = await GetCurrentStockAsync(productId, warehouseId);
        int recommendedOrderQuantity = CalculateRecommendedOrderQuantity(
            predictedDemand, 
            currentStock, 
            reorderSettings);

        // Tính ngày đề xuất đặt hàng
        DateTime? suggestedOrderDate = CalculateSuggestedOrderDate(
            currentStock, 
            predictedDemand, 
            reorderSettings);

        var forecast = new DemandForecast
        {
            ForecastId = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            ForecastPeriod = forecastPeriod,
            PredictedDemand = predictedDemand,
            Algorithm = algorithm,
            RecommendedOrderQuantity = recommendedOrderQuantity,
            SuggestedOrderDate = suggestedOrderDate,
            CreatedAt = DateTime.UtcNow,
            Product = null!, // Will be loaded by repository
            Warehouse = null! // Will be loaded by repository
        };

        await _forecastRepository.AddAsync(forecast);
        return forecast;
    }

    /// <summary>
    /// Cập nhật actual demand và tính accuracy
    /// </summary>
    public async Task UpdateActualDemandAsync(Guid forecastId, int actualDemand)
    {
        var forecast = await _forecastRepository.GetByIdAsync(forecastId);
        if (forecast == null)
        {
            throw new InvalidOperationException("Không tìm thấy dự báo.");
        }

        forecast.ActualDemand = actualDemand;
        
        // Tính độ chính xác
        if (forecast.PredictedDemand > 0)
        {
            var error = Math.Abs(forecast.PredictedDemand - actualDemand);
            forecast.Accuracy = Math.Max(0, 100 - (decimal)error / forecast.PredictedDemand * 100);
        }

        await _forecastRepository.UpdateAsync(forecast);
    }

    /// <summary>
    /// Lấy dữ liệu xuất kho lịch sử
    /// </summary>
    private async Task<List<int>> GetHistoricalExportDataAsync(
        Guid productId, 
        Guid warehouseId, 
        int months)
    {
        var result = new List<int>();
        var now = DateTime.UtcNow;

        for (int i = months; i > 0; i--)
        {
            var month = now.AddMonths(-i);
            var startDate = new DateTime(month.Year, month.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            // Tính tổng số lượng xuất trong tháng
            var exports = await _exportDetailRepository.GetByDateRangeAsync(startDate, endDate);
            var totalExported = exports
                .Where(e => e.ProductId == productId && e.Stock.WarehouseId == warehouseId)
                .Sum(e => e.Quantity);

            result.Add(totalExported);
        }

        return result;
    }

    /// <summary>
    /// Tính trung bình động (Moving Average)
    /// </summary>
    private int CalculateMovingAverage(List<int> data)
    {
        if (data.Count == 0) return 0;
        return (int)Math.Round(data.Average());
    }

    /// <summary>
    /// Tính trung bình động có trọng số (Weighted Moving Average)
    /// Tháng gần nhất có trọng số cao hơn
    /// </summary>
    private int CalculateWeightedMovingAverage(List<int> data)
    {
        if (data.Count == 0) return 0;

        decimal sum = 0;
        decimal weightSum = 0;

        for (int i = 0; i < data.Count; i++)
        {
            int weight = i + 1; // Tháng gần nhất có trọng số cao hơn
            sum += data[i] * weight;
            weightSum += weight;
        }

        return weightSum > 0 ? (int)Math.Round(sum / weightSum) : 0;
    }

    /// <summary>
    /// Tính làm mượt hàm mũ (Exponential Smoothing)
    /// Alpha = 0.3 (30% dữ liệu mới, 70% dữ liệu cũ)
    /// </summary>
    private int CalculateExponentialSmoothing(List<int> data, decimal alpha = 0.3m)
    {
        if (data.Count == 0) return 0;
        if (data.Count == 1) return data[0];

        decimal forecast = data[0];
        
        for (int i = 1; i < data.Count; i++)
        {
            forecast = alpha * data[i] + (1 - alpha) * forecast;
        }

        return (int)Math.Round(forecast);
    }

    /// <summary>
    /// Lấy tồn kho hiện tại
    /// </summary>
    private async Task<int> GetCurrentStockAsync(Guid productId, Guid warehouseId)
    {
        var stock = await _stockRepository.GetByProductAndWarehouseAsync(productId, warehouseId);
        return stock?.Quantity ?? 0;
    }

    /// <summary>
    /// Tính số lượng đề xuất đặt hàng
    /// </summary>
    private int CalculateRecommendedOrderQuantity(
        int predictedDemand, 
        int currentStock, 
        AutoReorderSettings? settings)
    {
        if (settings != null)
        {
            // Nếu có settings, dùng reorder quantity đã cấu hình
            if (currentStock < settings.ReorderPoint)
            {
                return settings.ReorderQuantity;
            }
            return 0;
        }
        else
        {
            // Nếu không có settings, đề xuất đủ để đáp ứng nhu cầu
            int shortage = predictedDemand - currentStock;
            return shortage > 0 ? shortage : 0;
        }
    }

    /// <summary>
    /// Tính ngày đề xuất đặt hàng
    /// </summary>
    private DateTime? CalculateSuggestedOrderDate(
        int currentStock, 
        int predictedDemand, 
        AutoReorderSettings? settings)
    {
        if (predictedDemand == 0) return null;

        int leadTimeDays = settings?.LeadTimeDays ?? 7;
        
        // Ước tính số ngày hàng tồn kho còn đủ dùng
        double dailyDemand = predictedDemand / 30.0; // Giả sử 1 tháng = 30 ngày
        double daysOfStock = currentStock / dailyDemand;

        // Nếu hàng tồn kho còn ít hơn lead time + safety buffer (3 ngày)
        if (daysOfStock < leadTimeDays + 3)
        {
            return DateTime.UtcNow;
        }

        // Ngày đề xuất = Ngày hàng hết - Lead time
        return DateTime.UtcNow.AddDays(daysOfStock - leadTimeDays);
    }
}
