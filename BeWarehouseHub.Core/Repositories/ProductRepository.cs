using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Core.Repositories;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Services;

public class ProductRepository : BaseRepository<Product>, IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context) : base(context)
    {
        _context = context;
    }

    public override async Task<IEnumerable<Product>> GetAllAsync()
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    public override async Task<Product?> GetByIdAsync(Guid id)
    {
        return await _context.Products
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .FirstOrDefaultAsync(p => p.ProductId == id);
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo kho
    /// </summary>
    public async Task<IEnumerable<Product>> GetByWarehouseIdAsync(Guid warehouseId)
    {
        var productIds = await _context.Stocks
            .Where(s => s.WarehouseId == warehouseId)
            .Select(s => s.ProductId)
            .Distinct()
            .ToListAsync();

        return await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo Category
    /// </summary>
    public async Task<IEnumerable<Product>> GetByCategoryIdAsync(Guid categoryId)
    {
        return await _context.Products
            .Where(p => p.CategoryId == categoryId)
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách sản phẩm theo ngày nhập (trong khoảng từ startDate đến endDate)
    /// </summary>
    public async Task<IEnumerable<Product>> GetByImportDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        var productIds = await _context.ImportDetails
            .Where(id => id.DateImport >= startDate && id.DateImport <= endDate)
            .Select(id => id.ProductId)
            .Distinct()
            .ToListAsync();

        return await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }

    /// <summary>
    /// Lấy danh sách sản phẩm nhập trong 1 ngày cụ thể
    /// </summary>
    public async Task<IEnumerable<Product>> GetByImportDateAsync(DateTime date)
    {
        var startOfDay = DateTime.UtcNow.Date;
        var endOfDay = startOfDay.AddDays(1).AddTicks(-1);
        
        var productIds = await _context.ImportDetails
            .Where(id => id.DateImport >= startOfDay && id.DateImport <= endOfDay)
            .Select(id => id.ProductId)
            .Distinct()
            .ToListAsync();

        return await _context.Products
            .Where(p => productIds.Contains(p.ProductId))
            .Include(p => p.Category)
            .Include(p => p.Supplier)
            .ToListAsync();
    }
}