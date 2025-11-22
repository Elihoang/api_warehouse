using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class StockService
{
    private readonly IStockRepository _stockRepository;

    public StockService(IStockRepository stockRepository)
    {
        _stockRepository = stockRepository;
    }

    public async Task<IEnumerable<Stock>> GetAllAsync()
        => await _stockRepository.GetAllAsync();

    public async Task<Stock?> GetByIdAsync(Guid warehouseId, Guid productId)
        => await _stockRepository.FindAsync(s => s.WarehouseId == warehouseId && s.ProductId == productId)
            .ContinueWith(t => t.Result.FirstOrDefault());

    public async Task AddAsync(Stock stock)
        => await _stockRepository.AddAsync(stock);

    public async Task UpdateAsync(Stock stock)
        => await _stockRepository.UpdateAsync(stock);

    public async Task DeleteAsync(Stock stock)
        => await _stockRepository.DeleteAsync(stock);
}