using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Share.DTOs.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = "Username là bắt buộc")]
    [RegularExpression(@"^[a-zA-Z0-9_]+$", ErrorMessage = "Username chỉ được chứa chữ cái không dấu, số và dấu gạch dưới (_), không được có khoảng trắng")]
    [MinLength(3, ErrorMessage = "Username phải có ít nhất 3 ký tự")]
    [MaxLength(50, ErrorMessage = "Username không được vượt quá 50 ký tự")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password là bắt buộc")]
    [MinLength(6, ErrorMessage = "Password phải có ít nhất 6 ký tự")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "Staff";
}