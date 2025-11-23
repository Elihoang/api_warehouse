using System.ComponentModel.DataAnnotations;
using BeWarehouseHub.Domain.Enums;

namespace BeWarehouseHub.Domain.Models;

public class User
{
    [Key]
    public Guid UserId { get; set; }

    [Required]
    [MaxLength(100)]
    public string UserName { get; set; }
    [Required]
    public string PasswordHash { get; set; } = string.Empty;
    
    public string Email { get; set; }
    
    public Role Role { get; set; }

    public ICollection<ImportReceipt> ImportReceipts { get; set; }
    public ICollection<ExportReceipt> ExportReceipts { get; set; }
}