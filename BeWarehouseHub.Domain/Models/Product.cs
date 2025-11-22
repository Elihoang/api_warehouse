using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class Product
{
    [Key]
    public Guid ProductId { get; set; }

    [Required]
    [MaxLength(150)]
    public string ProductName { get; set; }

    public Guid? CategoryId { get; set; }
    public Category Category { get; set; }

    public Guid? SupplierId { get; set; }
    public Supplier Supplier { get; set; }

    [MaxLength(50)]
    public string Unit { get; set; }

    [Column(TypeName = "numeric(18,2)")]
    public decimal Price { get; set; }
    
    public DateTime Time { get; set; }

    public ICollection<Stock> Stocks { get; set; }
    public ICollection<ImportDetail> ImportDetails { get; set; }
    public ICollection<ExportDetail> ExportDetails { get; set; }
}