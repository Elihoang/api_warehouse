using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BeWarehouseHub.Domain.Models;

public class Stock
{
    [Key]
    public Guid WarehouseId { get; set; }
    public Warehouse Warehouse { get; set; }
    
    public Guid ProductId { get; set; }
    public Product Product { get; set; }

    public int Quantity { get; set; }
}