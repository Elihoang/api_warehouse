using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Share.DTOs.DemandForecast;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class DemandForecastController : ControllerBase
{
    private readonly DemandForecastService _service;

    public DemandForecastController(DemandForecastService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả dự báo")]
    public async Task<IActionResult> GetAllAsync()
    {
        var forecasts = await _service.GetAllAsync();
        var result = forecasts.Select(f => MapToDto(f));
        return Ok(result);
    }

   [HttpGet("{id}", Name = "GetDemandForecastById")]
    [SwaggerOperation(Summary = "Lấy dự báo theo ID")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var forecast = await _service.GetByIdAsync(id);
        if (forecast == null)
            return NotFound(new { message = "Không tìm thấy dự báo" });

        return Ok(MapToDto(forecast));
    }

    [HttpGet("product/{productId}")]
    [SwaggerOperation(Summary = "Lấy dự báo theo sản phẩm")]
    public async Task<IActionResult> GetByProductIdAsync(Guid productId)
    {
        var forecasts = await _service.GetForecastsByProductIdAsync(productId);
        var result = forecasts.Select(f => MapToDto(f));
        return Ok(result);
    }

    [HttpGet("warehouse/{warehouseId}")]
    [SwaggerOperation(Summary = "Lấy dự báo theo kho")]
    public async Task<IActionResult> GetByWarehouseIdAsync(Guid warehouseId)
    {
        var forecasts = await _service.GetForecastsByWarehouseIdAsync(warehouseId);
        var result = forecasts.Select(f => MapToDto(f));
        return Ok(result);
    }

    [HttpGet("latest")]
    [SwaggerOperation(Summary = "Lấy dự báo mới nhất cho sản phẩm và kho")]
    public async Task<IActionResult> GetLatestForecastAsync([FromQuery] Guid productId, [FromQuery] Guid warehouseId)
    {
        var forecast = await _service.GetLatestForecastAsync(productId, warehouseId);
        if (forecast == null)
            return NotFound(new { message = "Không tìm thấy dự báo" });

        return Ok(MapToDto(forecast));
    }

    [HttpPost("generate")]
    [SwaggerOperation(Summary = "Tạo dự báo mới")]
    [SwaggerResponse(200, "Dự báo đã được tạo thành công")]
    [SwaggerResponse(400, "Dữ liệu đầu vào không hợp lệ")]
    public async Task<IActionResult> GenerateForecastAsync([FromBody] CreateDemandForecastDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Validate algorithm
        var validAlgorithms = new[] { "MovingAverage", "WeightedMovingAverage", "ExponentialSmoothing" };
        if (!validAlgorithms.Contains(dto.Algorithm))
        {
            return BadRequest(new
            {
                message = "Thuật toán không hợp lệ",
                validAlgorithms
            });
        }

        try
        {
            // Convert DateOnly to DateTime (ngày đầu tiên của kỳ dự báo, UTC)
            var forecastDateTime = dto.ForecastPeriod.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
            
            var forecast = await _service.GenerateForecastAsync(
                dto.ProductId,
                dto.WarehouseId,
                forecastDateTime,
                dto.Algorithm,
                lookbackMonths: 3
            );

            var result = MapToDto(forecast);
            return CreatedAtRoute("GetDemandForecastById", new { id = forecast.ForecastId }, result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPatch("{id}/actual-demand")]
    [SwaggerOperation(Summary = "Cập nhật nhu cầu thực tế và tính độ chính xác")]
    public async Task<IActionResult> UpdateActualDemandAsync(Guid id, [FromBody] int actualDemand)
    {
        try
        {
            await _service.UpdateActualDemandAsync(id, actualDemand);
            
            var updated = await _service.GetByIdAsync(id);
            return Ok(new
            {
                message = "Đã cập nhật nhu cầu thực tế",
                forecast = MapToDto(updated!)
            });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    [HttpGet("algorithms")]
    [SwaggerOperation(Summary = "Lấy danh sách thuật toán dự báo có sẵn")]
    public IActionResult GetAlgorithms()
    {
        return Ok(new
        {
            algorithms = new[]
            {
                new
                {
                    name = "MovingAverage",
                    displayName = "Trung bình động",
                    description = "Đơn giản, phù hợp với nhu cầu ổn định",
                    formula = "Forecast = Average(last N months)"
                },
                new
                {
                    name = "WeightedMovingAverage",
                    displayName = "Trung bình động có trọng số",
                    description = "Ưu tiên dữ liệu gần hơn",
                    formula = "Forecast = (w1×m1 + w2×m2 + w3×m3) / (w1 + w2 + w3)"
                },
                new
                {
                    name = "ExponentialSmoothing",
                    displayName = "Làm mượt hàm mũ",
                    description = "Thích nghi với xu hướng thay đổi",
                    formula = "Forecast = α × Actual + (1-α) × PreviousForecast"
                }
            }
        });
    }

    // Helper method
    private DemandForecastDto MapToDto(BeWarehouseHub.Domain.Models.DemandForecast forecast)
    {
        // Get current stock (would need StockRepository injected for real implementation)
        // For now, just return 0
        int currentStock = 0;
        bool needReorder = forecast.RecommendedOrderQuantity > 0;
        int daysUntilReorder = forecast.SuggestedOrderDate.HasValue
            ? Math.Max(0, (forecast.SuggestedOrderDate.Value - DateTime.UtcNow).Days)
            : 0;

        return new DemandForecastDto
        {
            ForecastId = forecast.ForecastId,
            ProductId = forecast.ProductId,
            ProductName = forecast.Product?.ProductName,
            WarehouseId = forecast.WarehouseId,
            WarehouseName = forecast.Warehouse?.WarehouseName,
            ForecastPeriod = forecast.ForecastPeriod,
            PredictedDemand = forecast.PredictedDemand,
            ActualDemand = forecast.ActualDemand,
            Accuracy = forecast.Accuracy,
            Algorithm = forecast.Algorithm,
            RecommendedOrderQuantity = forecast.RecommendedOrderQuantity,
            SuggestedOrderDate = forecast.SuggestedOrderDate,
            CreatedAt = forecast.CreatedAt,
            Note = forecast.Note,
            CurrentStock = currentStock,
            NeedReorder = needReorder,
            DaysUntilReorder = daysUntilReorder
        };
    }
}
