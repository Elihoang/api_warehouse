namespace BeWarehouseHub.Share.DTOs.Auth;

public record LoginRequestDto(
    string LoginIdentifier, // Username hoặc Email
    string Password
);


