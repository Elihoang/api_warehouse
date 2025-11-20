using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class ExportReceipt
{
    [Key]
    public Guid ExportId { get; set; }
    public DateTime ExportDate { get; set; }

    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; }

    public ICollection<ExportDetail> ExportDetails { get; set; }
}