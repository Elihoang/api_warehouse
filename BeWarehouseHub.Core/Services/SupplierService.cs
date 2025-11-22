
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class SupplierService
{
    private readonly ISupplierRepository _supplierRepository;

    public SupplierService(ISupplierRepository supplierRepository)
    {
        _supplierRepository = supplierRepository;
    }

    public async Task<IEnumerable<Supplier>> GetAllAsync()
        => await _supplierRepository.GetAllAsync();

    public async Task<Supplier?> GetByIdAsync(Guid id)
        => await _supplierRepository.GetByIdAsync(id);

    public async Task AddAsync(Supplier supplier)
        => await _supplierRepository.AddAsync(supplier);

    public async Task UpdateAsync(Supplier supplier)
        => await _supplierRepository.UpdateAsync(supplier);

    public async Task DeleteAsync(Supplier supplier)
        => await _supplierRepository.DeleteAsync(supplier);
}