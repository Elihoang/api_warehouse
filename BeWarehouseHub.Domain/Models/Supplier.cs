using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class Supplier
{
    [Key]
    public Guid SupplierId { get; set; }

    [Required]
    [MaxLength(150)]
    public string SupplierName { get; set; }

    [MaxLength(20)]
    public string Phone { get; set; }

    [MaxLength(255)]
    public string Address { get; set; }

    public ICollection<Product> Products { get; set; }
}