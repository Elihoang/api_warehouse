using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class ImportDetail
{
    [Key]
    public Guid ImportDetailId { get; set; }

    public Guid ImportId { get; set; }
    [ForeignKey("ImportId")]
    public virtual ImportReceipt ImportReceipt { get; set; }
    public Guid ProductId { get; set; }
    public Product Product { get; set; }
    public Guid StockId { get; set; }      
    public Stock Stock { get; set; }  
    
    public int Quantity { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal Price { get; set; }
    public DateTime DateImport { get; set; }

    // Quản lý lô hàng
    public Guid? BatchId { get; set; }
    public ProductBatch? Batch { get; set; }
}