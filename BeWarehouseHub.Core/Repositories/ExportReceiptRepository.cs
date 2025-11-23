using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Repositories;

public class ExportReceiptRepository : BaseRepository<ExportReceipt>, IExportReceiptRepository
{
    public ExportReceiptRepository(AppDbContext context) : base(context) { }
}