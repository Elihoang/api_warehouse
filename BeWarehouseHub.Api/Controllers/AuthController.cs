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
    /// Đăng nhập hệ thống (hỗ trợ username hoặc email)
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
            return BadRequest(new { message = "Dữ liệu không hợp lệ" });

        // Tìm user theo username HOẶC email
        var user = await FindUserByLoginIdentifierAsync(request.LoginIdentifier);
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Tên đăng nhập/Email hoặc mật khẩu không đúng" });

        // Kiểm tra trạng thái tài khoản
        if (user.Status == UserStatus.Locked)
            return Unauthorized(new { message = "Tài khoản đã bị khóa. Vui lòng liên hệ quản trị viên." });
        
        if (user.Status == UserStatus.Inactive)
            return Unauthorized(new { message = "Tài khoản đã ngừng hoạt động." });
        
        if (user.Status == UserStatus.Pending)
            return Unauthorized(new { message = "Tài khoản chưa được kích hoạt. Vui lòng liên hệ quản trị viên." });

        // Cập nhật LastLoginAt
        user.LastLoginAt = DateTime.UtcNow;
        await _userService.UpdateAsync(user);

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
                user.Email,
                user.FullName,
                user.AvatarUrl,
                role = user.Role.ToString(),
                status = user.Status.ToString()
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

        // Kiểm tra username đã tồn tại
        var existingUserByUsername = await FindUserByUsernameAsync(request.UserName);
        if (existingUserByUsername != null)
            return Conflict(new { message = "Username đã được sử dụng" });

        // Kiểm tra email đã tồn tại
        var existingUserByEmail = await FindUserByEmailAsync(request.Email);
        if (existingUserByEmail != null)
            return Conflict(new { message = "Email đã được sử dụng" });

        if (!Enum.TryParse<Role>(request.Role, true, out var role))
            return BadRequest(new { message = "Role không hợp lệ. Chỉ chấp nhận: Staff, Manager, Admin" });

        var newUser = new User
        {
            UserId = Guid.NewGuid(),
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim().ToLower(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = role
        };

        await _userService.AddAsync(newUser);

        return Ok(new
        {
            message = "Đăng ký thành công",
            userId = newUser.UserId,
            userName = newUser.UserName,
            email = newUser.Email,
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

    // === Helper Methods ===
    
    /// <summary>
    /// Tìm user theo username HOẶC email
    /// </summary>
    private async Task<User?> FindUserByLoginIdentifierAsync(string loginIdentifier)
    {
        var users = await _userService.GetAllAsync();
        return users.FirstOrDefault(u => 
            u.UserName.Equals(loginIdentifier, StringComparison.OrdinalIgnoreCase) ||
            u.Email.Equals(loginIdentifier, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tìm user theo username
    /// </summary>
    private async Task<User?> FindUserByUsernameAsync(string userName)
    {
        var users = await _userService.GetAllAsync();
        return users.FirstOrDefault(u => 
            u.UserName.Equals(userName, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Tìm user theo email
    /// </summary>
    private async Task<User?> FindUserByEmailAsync(string email)
    {
        var users = await _userService.GetAllAsync();
        return users.FirstOrDefault(u => 
            u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
    }
}

// DTO cho Refresh Token
