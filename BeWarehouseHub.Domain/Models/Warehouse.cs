using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class Warehouse
{
    [Key]
    public Guid WarehouseId { get; set; }

    [Required]
    [MaxLength(150)]
    public string WarehouseName { get; set; }

    [MaxLength(255)]
    public string Location { get; set; }

    public ICollection<Stock> Stocks { get; set; }
    public ICollection<ImportReceipt> ImportReceipts { get; set; }
    public ICollection<ExportReceipt> ExportReceipts { get; set; }
}