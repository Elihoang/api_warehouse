using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Configurations;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; }
    public DbSet<Supplier> Suppliers { get; set; }
    public DbSet<Warehouse> Warehouses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Stock> Stocks { get; set; }
    public DbSet<ImportReceipt> ImportReceipts { get; set; }
    public DbSet<ImportDetail> ImportDetails { get; set; }
    public DbSet<ExportReceipt> ExportReceipts { get; set; }
    public DbSet<ExportDetail> ExportDetails { get; set; }
    
    // New features
    public DbSet<ProductBatch> ProductBatches { get; set; }
    public DbSet<InventoryAudit> InventoryAudits { get; set; }
    public DbSet<InventoryAuditDetail> InventoryAuditDetails { get; set; }
    public DbSet<DemandForecast> DemandForecasts { get; set; }
    public DbSet<AutoReorderSettings> AutoReorderSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<User>()
            .Property(u => u.Role)
            .HasConversion<string>();
             modelBuilder.Entity<UserWarehouse>()
        .HasKey(x => new { x.UserId, x.WarehouseId });

    modelBuilder.Entity<UserWarehouse>()
        .HasOne(x => x.User)
        .WithMany(u => u.UserWarehouses)
        .HasForeignKey(x => x.UserId);

    modelBuilder.Entity<UserWarehouse>()
        .HasOne(x => x.Warehouse)
        .WithMany(w => w.UserWarehouses)
        .HasForeignKey(x => x.WarehouseId);

    }
}

