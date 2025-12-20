using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class ExportDetail
{
    [Key]
    public Guid ExportDetailId { get; set; }

    public Guid ExportId { get; set; }
    [ForeignKey("ExportId")]
    public virtual ExportReceipt ExportReceipt { get; set; }
    public Guid StockId { get; set; }      
    public Stock Stock { get; set; }  

    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
    public DateTime DateExport { get; set; }
}