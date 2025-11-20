using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class Category
{
    [Key]
    public Guid CategoryId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; }

    public ICollection<Product> Products { get; set; }
}