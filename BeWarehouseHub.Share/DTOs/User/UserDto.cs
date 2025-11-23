using BeWarehouseHub.Domain.Enums;

namespace BeWarehouseHub.Share.DTOs.User;

public class UserDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Role Role { get; set; }
    public int ImportCount { get; set; }
    public int ExportCount { get; set; }
}