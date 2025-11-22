// Core/Services/ProductService.cs
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class ProductService
{
    private readonly IProductRepository _productRepository;

    public ProductService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<IEnumerable<Product>> GetAllAsync()
        => await _productRepository.GetAllAsync();

    public async Task<Product?> GetByIdAsync(Guid id)
        => await _productRepository.GetByIdAsync(id);

    public async Task AddAsync(Product product)
        => await _productRepository.AddAsync(product);

    public async Task UpdateAsync(Product product)
        => await _productRepository.UpdateAsync(product);

    public async Task DeleteAsync(Product product)
        => await _productRepository.DeleteAsync(product);
}