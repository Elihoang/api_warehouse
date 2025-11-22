using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IProductRepository : IRepository<Product>
{
    // Nếu sau này cần query đặc biệt cho Product thì thêm ở đây
    // Ví dụ: Task<IEnumerable<Product>> GetProductsWithCategoryAsync();
}