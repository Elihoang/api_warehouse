using System.ComponentModel.DataAnnotations;

namespace BeWarehouseHub.Domain.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; }

    [MaxLength(50)]
    public string Role { get; set; }

    public ICollection<ImportReceipt> ImportReceipts { get; set; }
    public ICollection<ExportReceipt> ExportReceipts { get; set; }
}