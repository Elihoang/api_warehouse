// Api/Controllers/AuthController.cs

using System.Security.Claims;
using BeWarehouseHub.Core.Helpers;
using BeWarehouseHub.Core.Services;
using BeWarehouseHub.Domain.Enums;
using BeWarehouseHub.Domain.Models;
using BeWarehouseHub.Share.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using BCrypt.Net;

namespace BeWarehouseHub.Api.Controllers;

[Route("api/auth")]
[ApiController]
[AllowAnonymous] // Cho phép truy cập không cần token
public class AuthController : ControllerBase
{
    private readonly UserService _userService;
    private readonly IConfiguration _config;

    public AuthController(UserService userService, IConfiguration config)
    {
        _userService = userService;
        _config = config;
    }

    /// <summary>
    /// Đăng nhập hệ thống
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        var user = await FindUserByUserNameAsync(request.UserName);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Tên đăng nhập hoặc mật khẩu không đúng" });

        var accessToken = JwtHelper.GenerateAccessToken(user.UserId, user.UserName, user.Role);
        var refreshToken = JwtHelper.GenerateRefreshToken();

        // TODO: Lưu refreshToken vào DB (khuyên dùng bảng RefreshTokens)
        // await _userService.SaveRefreshTokenAsync(user.UserId, refreshToken);

        return Ok(new
        {
            message = "Đăng nhập thành công",
            accessToken,
            refreshToken,
            expiresIn = int.Parse(_config["Jwt:AccessTokenExpireMinutes"]!),
            user = new
            {
                user.UserId,
                user.UserName,
                role = user.Role.ToString(), 
            }
        });
    }

    /// <summary>
    /// Đăng ký tài khoản mới (chỉ Admin mới được phép - hoặc mở cho ai cũng được)
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var existingUser = await FindUserByUserNameAsync(request.UserName);
        if (existingUser != null)
            return Conflict(new { message = "Tên đăng nhập đã được sử dụng" });

        if (!Enum.TryParse<Role>(request.Role, true, out var role))
            return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận: Staff, Manager, Admin" });

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role
        };

        await _userService.AddAsync(newUser);

        return Ok(new
        {
            message = "Đăng ký thành công",
            userId = newUser.UserId,
            userName = newUser.UserName,
            role = newUser.Role.ToString()
        });
    }

    /// <summary>
    /// Làm mới Access Token bằng Refresh Token
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken))
            return BadRequest(new { message = "Refresh token là bắt buộc" });

        try
        {
            var principal = JwtHelper.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdClaim = principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized(new { message = "Token không hợp lệ" });

            var user = await _userService.GetByIdAsync(userId);
            if (user == null)
                return Unauthorized(new { message = "Người dùng không tồn tại" });

            // TODO: Kiểm tra refreshToken trong DB có khớp không
            // if (!await _userService.IsRefreshTokenValid(userId, request.RefreshToken))
            //     return Unauthorized(new { message = "Refresh token không hợp lệ hoặc đã bị thu hồi" });

            var newAccessToken = JwtHelper.GenerateAccessToken(user.UserId, user.UserName, user.Role);
            var newRefreshToken = JwtHelper.GenerateRefreshToken();

            return Ok(new
            {
                accessToken = newAccessToken,
                refreshToken = newRefreshToken,
                expiresIn = int.Parse(_config["Jwt:AccessTokenExpireMinutes"]!)
            });
        }
        catch (Exception)
        {
            return Unauthorized(new { message = "Token không hợp lệ hoặc đã hết hạn" });
        }
    }

    // === Helper riêng trong controller (gợi ý) ===
    private async Task<User?> FindUserByUserNameAsync(string userName)
    {
        var users = await _userService.GetAllAsync();
        return users.FirstOrDefault(u => 
            u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }
}

// DTO cho Refresh Token
