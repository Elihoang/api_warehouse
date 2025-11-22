using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Stock;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class StockController : ControllerBase
{
    private readonly StockService _service;

    public StockController(StockService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy toàn bộ tồn kho (tất cả kho + tất cả sản phẩm)")]
    public async Task<IActionResult> GetAllAsync()
    {
        var stocks = await _service.GetAllAsync();

        var result = stocks.Select(s => new StockDto
        {
            WarehouseId = s.WarehouseId,
            WarehouseName = s.Warehouse?.WarehouseName ?? "Không xác định",
            ProductId = s.ProductId,
            ProductName = s.Product?.ProductName ?? "Không xác định",
            Quantity = s.Quantity
        });

        return Ok(result);
    }

    [HttpGet("{warehouseId}/{productId}")]
    [SwaggerOperation(Summary = "Lấy tồn kho của 1 sản phẩm trong 1 kho cụ thể")]
    public async Task<IActionResult> GetByIdAsync(Guid warehouseId, Guid productId)
    {
        var stock = await _service.GetByIdAsync(warehouseId, productId);
        if (stock == null)
            return NotFound(new { message = "Không tìm thấy tồn kho" });

        var dto = new StockDto
        {
            WarehouseId = stock.WarehouseId,
            WarehouseName = stock.Warehouse?.WarehouseName ?? "",
            ProductId = stock.ProductId,
            ProductName = stock.Product?.ProductName ?? "",
            Quantity = stock.Quantity
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Thêm/Cập nhật tồn kho (Upsert)")]
    public async Task<IActionResult> UpsertAsync([FromBody] CreateStockDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existing = await _service.GetByIdAsync(dto.WarehouseId, dto.ProductId);

        if (existing == null)
        {
            var stock = new Stock
            {
                WarehouseId = dto.WarehouseId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity
            };
            await _service.AddAsync(stock);

            return Ok(new { message = "Thêm tồn kho thành công", quantity = stock.Quantity });
        }
        else
        {
            existing.Quantity = dto.Quantity;
            await _service.UpdateAsync(existing);
            return Ok(new { message = "Cập nhật tồn kho thành công", quantity = existing.Quantity });
        }
    }

    [HttpPatch]
    [SwaggerOperation(Summary = "Cập nhật số lượng tồn kho (chỉ thay đổi Quantity)")]
    public async Task<IActionResult> UpdateQuantityAsync([FromBody] UpdateStockDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var stock = await _service.GetByIdAsync(dto.WarehouseId, dto.ProductId);
        if (stock == null)
            return NotFound(new { message = "Không tìm thấy tồn kho" });

        stock.Quantity = dto.Quantity;
        await _service.UpdateAsync(stock);

        return NoContent();
    }

    [HttpDelete("{warehouseId}/{productId}")]
    [SwaggerOperation(Summary = "Xóa tồn kho của sản phẩm trong kho")]
    public async Task<IActionResult> DeleteAsync(Guid warehouseId, Guid productId)
    {
        var stock = await _service.GetByIdAsync(warehouseId, productId);
        if (stock == null)
            return NotFound(new { message = "Không tìm thấy tồn kho" });

        await _service.DeleteAsync(stock);
        return NoContent();
    }
}