using System.ComponentModel.DataAnnotations;
using BeWarehouseHub.Domain.Enums;

namespace BeWarehouseHub.Share.DTOs.User;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? FullName { get; set; }

    public string Email { get; set; } = string.Empty;

    public string? AvatarUrl { get; set; }

    public Role Role { get; set; }= Role.Staff;
}