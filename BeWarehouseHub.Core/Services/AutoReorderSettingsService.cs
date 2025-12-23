using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class AutoReorderSettingsService
{
    private readonly IAutoReorderSettingsRepository _settingsRepository;
    private readonly IStockRepository _stockRepository;

    public AutoReorderSettingsService(
        IAutoReorderSettingsRepository settingsRepository,
        IStockRepository stockRepository)
    {
        _settingsRepository = settingsRepository;
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<AutoReorderSettings>> GetAllAsync()
        => await _settingsRepository.GetAllAsync();

    public async Task<AutoReorderSettings?> GetByIdAsync(Guid id)
        => await _settingsRepository.GetByIdAsync(id);

    public async Task<AutoReorderSettings?> GetByProductAndWarehouseAsync(Guid productId, Guid warehouseId)
        => await _settingsRepository.GetByProductAndWarehouseAsync(productId, warehouseId);

    public async Task<IEnumerable<AutoReorderSettings>> GetEnabledSettingsAsync()
        => await _settingsRepository.GetEnabledSettingsAsync();

    public async Task<IEnumerable<AutoReorderSettings>> GetByWarehouseIdAsync(Guid warehouseId)
        => await _settingsRepository.GetByWarehouseIdAsync(warehouseId);

    public async Task AddAsync(AutoReorderSettings settings)
    {
        // Validate logic: ReorderPoint phải >= MinStock và <= MaxStock
        if (settings.ReorderPoint < settings.MinStockLevel)
        {
            throw new InvalidOperationException("Điểm đặt hàng lại phải lớn hơn hoặc bằng mức tồn tối thiểu.");
        }

        if (settings.ReorderPoint >= settings.MaxStockLevel)
        {
            throw new InvalidOperationException("Điểm đặt hàng lại phải nhỏ hơn mức tồn tối đa.");
        }

        if (settings.ReorderQuantity <= 0)
        {
            throw new InvalidOperationException("Số lượng đặt hàng phải lớn hơn 0.");
        }

        // Kiểm tra xem đã có settings cho product-warehouse này chưa
        var existing = await _settingsRepository.GetByProductAndWarehouseAsync(
            settings.ProductId, 
            settings.WarehouseId);

        if (existing != null)
        {
            throw new InvalidOperationException("Đã tồn tại cài đặt tự động đặt hàng cho sản phẩm này tại kho này.");
        }

        await _settingsRepository.AddAsync(settings);
    }

    public async Task UpdateAsync(AutoReorderSettings settings)
    {
        // Validate logic giống như Add
        if (settings.ReorderPoint < settings.MinStockLevel)
        {
            throw new InvalidOperationException("Điểm đặt hàng lại phải lớn hơn hoặc bằng mức tồn tối thiểu.");
        }

        if (settings.ReorderPoint >= settings.MaxStockLevel)
        {
            throw new InvalidOperationException("Điểm đặt hàng lại phải nhỏ hơn mức tồn tối đa.");
        }

        settings.UpdatedAt = DateTime.UtcNow;
        await _settingsRepository.UpdateAsync(settings);
    }

    public async Task DeleteAsync(AutoReorderSettings settings)
        => await _settingsRepository.DeleteAsync(settings);

    /// <summary>
    /// Kiểm tra các sản phẩm cần đặt hàng lại
    /// </summary>
    public async Task<List<ReorderRecommendation>> CheckReorderNeedsAsync()
    {
        var recommendations = new List<ReorderRecommendation>();
        var enabledSettings = await _settingsRepository.GetEnabledSettingsAsync();

        foreach (var setting in enabledSettings)
        {
            var stock = await _stockRepository.GetByProductAndWarehouseAsync(
                setting.ProductId, 
                setting.WarehouseId);

            if (stock != null && stock.Quantity <= setting.ReorderPoint)
            {
                recommendations.Add(new ReorderRecommendation
                {
                    ProductId = setting.ProductId,
                    ProductName = setting.Product.ProductName,
                    WarehouseId = setting.WarehouseId,
                    WarehouseName = setting.Warehouse.WarehouseName,
                    CurrentStock = stock.Quantity,
                    ReorderPoint = setting.ReorderPoint,
                    RecommendedQuantity = setting.ReorderQuantity,
                    MinStockLevel = setting.MinStockLevel,
                    MaxStockLevel = setting.MaxStockLevel,
                    LeadTimeDays = setting.LeadTimeDays,
                    SuggestedOrderDate = DateTime.UtcNow,
                    ExpectedDeliveryDate = DateTime.UtcNow.AddDays(setting.LeadTimeDays)
                });
            }
        }

        return recommendations;
    }

    /// <summary>
    /// Tính toán reorder settings tự động dựa trên lịch sử
    /// </summary>
    public async Task<AutoReorderSettings> SuggestSettingsAsync(
        Guid productId, 
        Guid warehouseId,
        int lookbackDays = 90)
    {
        // Lấy lịch sử xuất kho
        var startDate = DateTime.UtcNow.AddDays(-lookbackDays);
        var exports = await GetHistoricalExportsAsync(productId, warehouseId, startDate);

        if (!exports.Any())
        {
            // Không có data, trả về settings mặc định
            return CreateDefaultSettings(productId, warehouseId);
        }

        // Tính trung bình xuất hàng mỗi ngày
        double avgDailyDemand = exports.Sum() / (double)lookbackDays;

        // Tính độ lệch chuẩn để xác định safety stock
        double variance = exports.Select(x => Math.Pow(x - avgDailyDemand, 2)).Average();
        double stdDev = Math.Sqrt(variance);

        // Safety stock = 1.65 * std dev (95% service level)
        int safetyStock = (int)Math.Ceiling(1.65 * stdDev);

        // Lead time demand
        int leadTimeDays = 7; // Mặc định 7 ngày
        int leadTimeDemand = (int)Math.Ceiling(avgDailyDemand * leadTimeDays);

        // Reorder Point = Lead Time Demand + Safety Stock
        int reorderPoint = leadTimeDemand + safetyStock;

        // Min Stock = Safety Stock
        int minStock = safetyStock;

        // Reorder Quantity = Trung bình nhu cầu 30 ngày
        int reorderQuantity = (int)Math.Ceiling(avgDailyDemand * 30);

        // Max Stock = Reorder Point + Reorder Quantity
        int maxStock = reorderPoint + reorderQuantity;

        return new AutoReorderSettings
        {
            SettingId = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            MinStockLevel = minStock,
            ReorderPoint = reorderPoint,
            ReorderQuantity = reorderQuantity,
            MaxStockLevel = maxStock,
            LeadTimeDays = leadTimeDays,
            IsAutoReorderEnabled = true,
            CreatedAt = DateTime.UtcNow,
            Product = null!, // Will be loaded
            Warehouse = null! // Will be loaded
        };
    }

    private AutoReorderSettings CreateDefaultSettings(Guid productId, Guid warehouseId)
    {
        return new AutoReorderSettings
        {
            SettingId = Guid.NewGuid(),
            ProductId = productId,
            WarehouseId = warehouseId,
            MinStockLevel = 10,
            ReorderPoint = 20,
            ReorderQuantity = 100,
            MaxStockLevel = 200,
            LeadTimeDays = 7,
            IsAutoReorderEnabled = false, // Tắt mặc định do không có data
            CreatedAt = DateTime.UtcNow,
            Product = null!,
            Warehouse = null!
        };
    }

    private async Task<List<int>> GetHistoricalExportsAsync(
        Guid productId, 
        Guid warehouseId, 
        DateTime startDate)
    {
        // This would need IExportDetailRepository injected
        // For now, return empty list - to be implemented
        return new List<int>();
    }
}

/// <summary>
/// DTO for reorder recommendations
/// </summary>
public class ReorderRecommendation
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public Guid WarehouseId { get; set; }
    public string WarehouseName { get; set; } = string.Empty;
    public int CurrentStock { get; set; }
    public int ReorderPoint { get; set; }
    public int RecommendedQuantity { get; set; }
    public int MinStockLevel { get; set; }
    public int MaxStockLevel { get; set; }
    public int LeadTimeDays { get; set; }
    public DateTime SuggestedOrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
}
