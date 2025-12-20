using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    /// <summary>
    /// Lấy danh sách sản phẩm theo kho
    /// </summary>
    Task<IEnumerable<Product>> GetByWarehouseIdAsync(Guid warehouseId);
    
    /// <summary>
    /// Lấy danh sách sản phẩm theo Category
    /// </summary>
    Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId);
    
    /// <summary>
    /// Lấy danh sách sản phẩm theo ngày nhập (trong khoảng từ startDate đến endDate)
    /// </summary>
    Task<IEnumerable<Product>> GetByImportDateRangeAsync(DateTime startDate, DateTime endDate);
    
    /// <summary>
    /// Lấy danh sách sản phẩm nhập trong 1 ngày cụ thể
    /// </summary>
    Task<IEnumerable<Product>> GetByImportDateAsync(DateTime date);
}