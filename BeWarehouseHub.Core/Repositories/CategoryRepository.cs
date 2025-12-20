using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class CategoryRepository : BaseRepository<Category>,ICategoryRepository
{
    public CategoryRepository(AppDbContext context) : base(context)
    {
    }

    // Override để Include Products khi lấy tất cả categories
    public override async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Set<Category>()
            .Include(c => c.Products)
            .ToListAsync();
    }

    // Override để Include Products khi lấy category theo ID
    public override async Task<Category?> GetByIdAsync(Guid id)
    {
        return await _context.Set<Category>()
            .Include(c => c.Products)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }
}