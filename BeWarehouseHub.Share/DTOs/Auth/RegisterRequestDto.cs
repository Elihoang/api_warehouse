namespace BeWarehouseHub.Share.DTOs.Auth;

public record RegisterRequestDto(
    string UserName,
    string Password,
    string Email,
    string Role = "Staff"
);