using System.Linq.Expressions;
using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Services;

public class CategoryService(ICategoryRepository categoryRepository)
{
    private readonly ICategoryRepository _categoryRepository = categoryRepository;
    
    public async Task<IEnumerable<Category>> GetAllCateAsync()
    {
        return await  _categoryRepository.GetAllAsync();
    }

    public async Task<Category?> GetCateByIdAsync(Guid id)
    {
        return await  _categoryRepository.GetByIdAsync(id);
    }
    

    public async Task AddCateAsync(Category khoa)
    {
        await  _categoryRepository.AddAsync(khoa);
    }

    public async Task UpdateCateAsync(Category khoa)
    {
        await _categoryRepository.UpdateAsync(khoa);
    }

    public async Task DeleteCateAsync(Category khoa)
    {
        await  _categoryRepository.DeleteAsync(khoa);
    }

    public async Task<IEnumerable<Category>> FindCateAsync(Expression<Func<Category, bool>> predicate)
    {
        return await _categoryRepository.FindAsync(predicate);
    }
}