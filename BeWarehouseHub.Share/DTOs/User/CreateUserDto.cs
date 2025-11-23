using System.ComponentModel.DataAnnotations;
using BeWarehouseHub.Domain.Enums;

namespace BeWarehouseHub.Share.DTOs.User;

public class CreateUserDto
{
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public Role Role { get; set; }= Role.Staff;
}