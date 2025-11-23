namespace BeWarehouseHub.Share.DTOs.Auth;

public record LoginResponseDto(
    string Token,
    Guid UserId,
    string UserName,
    string Role
);