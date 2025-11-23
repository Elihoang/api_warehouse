// Api/Controllers/UserController.cs
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.User;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly UserService _service;

    public UserController(UserService service)
    {
        _service = service;
    }

    [HttpGet]
    [SwaggerOperation(Summary = "Lấy danh sách tất cả người dùng")]
    public async Task<IActionResult> GetAllAsync()
    {
        var users = await _service.GetAllAsync();

        var result = users.Select(u => new UserDto
        {
            UserId = u.UserId,
            UserName = u.UserName,
            Email = u.Email,
            Role = u.Role,
            ImportCount = u.ImportReceipts?.Count ?? 0,
            ExportCount = u.ExportReceipts?.Count ?? 0
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Lấy thông tin người dùng theo Id")]
    public async Task<IActionResult> GetByIdAsync(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        var dto = new UserDto
        {
            UserId = user.UserId,
            UserName = user.UserName,
            Email = user.Email,
            Role = user.Role ,
            ImportCount = user.ImportReceipts?.Count ?? 0,
            ExportCount = user.ExportReceipts?.Count ?? 0
        };

        return Ok(dto);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Tạo người dùng mới")]
    public async Task<IActionResult> CreateAsync([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new User
        {
            UserName = dto.UserName,
            Role = dto.Role
        };

        var created = await _service.AddAsync(user);

        var result = new UserDto
        {
            UserId = created.UserId,
            UserName = created.UserName,
            Email = created.Email,
            Role = created.Role,
            ImportCount = 0,
            ExportCount = 0
        };

        return Ok(result);
    }

    [HttpPatch("{id}")]
    [SwaggerOperation(Summary = "Cập nhật thông tin người dùng")]
    public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (id != dto.UserId)
            return BadRequest(new { message = "Id không khớp" });

        var existing = await _service.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { message = "Không tìm thấy người dùng" });

        existing.UserName = dto.UserName;
        existing.Email = dto.Email;
        existing.Role = dto.Role;

        await _service.UpdateAsync(existing);
        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Xóa người dùng (chỉ khi chưa lập phiếu nào)")]
    public async Task<IActionResult> DeleteAsync(Guid id)
    {
        var user = await _service.GetByIdAsync(id);
        if (user == null)
        {
            return NotFound();
        }

        await _service.DeleteAsync(user);
        return NoContent();
    }
}
        