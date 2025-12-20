namespace BeWarehouseHub.Domain.Enums;

public enum UserStatus
{
    Active = 1,        // Đang hoạt động
    Inactive = 2,      // Ngừng hoạt động (nghỉ việc)
    Locked = 3,        // Bị khóa (vi phạm / sai mật khẩu nhiều lần)
    Pending = 4        // Chưa kích hoạt
}
