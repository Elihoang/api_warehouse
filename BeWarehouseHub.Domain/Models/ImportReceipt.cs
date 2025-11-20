using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class ImportReceipt
{
    [Key]
    public Guid ImportId { get; set; }
    public DateTime ImportDate { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public ICollection<ImportDetail> ImportDetails { get; set; }
}