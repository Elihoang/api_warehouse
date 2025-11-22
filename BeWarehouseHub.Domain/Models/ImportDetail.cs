using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class ImportDetail
{
    [Key]
    public Guid ImportDetailId { get; set; }

    public Guid ImportId { get; set; }
    public ImportReceipt ImportReceipt { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal Price { get; set; }
    public DateTime DateImport { get; set; }
}