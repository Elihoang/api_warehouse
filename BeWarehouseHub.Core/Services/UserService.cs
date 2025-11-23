
using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Services;

public class UserService
{
    private readonly IUserRepository _userRepository;
    private readonly AppDbContext _context;

    public UserService(IUserRepository userRepository, AppDbContext context)
    {
        _userRepository = userRepository;
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users
            .Include(u => u.ImportReceipts)
            .Include(u => u.ExportReceipts)
            .OrderBy(u => u.UserName)
            .ToListAsync();
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .Include(u => u.ImportReceipts)
            .Include(u => u.ExportReceipts)
            .FirstOrDefaultAsync(u => u.UserId == id);
    }

    public async Task<User> AddAsync(User user)
    {
        user.UserId = Guid.NewGuid();
        await _userRepository.AddAsync(user);
        return user;
    }

    public async Task UpdateAsync(User user)
        => await _userRepository.UpdateAsync(user);

    public async Task DeleteAsync(User user)
        => await _userRepository.DeleteAsync(user);
}