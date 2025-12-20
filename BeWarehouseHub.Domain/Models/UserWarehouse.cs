namespace BeWarehouseHub.Domain.Models;

public class UserWarehouse
{
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public Guid WarehouseId { get; set; }
    public virtual Warehouse Warehouse { get; set; } = null!;
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
