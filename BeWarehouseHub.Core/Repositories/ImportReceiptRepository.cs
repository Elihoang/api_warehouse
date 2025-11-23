using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Repositories;

public class ImportReceiptRepository : BaseRepository<ImportReceipt>, IImportReceiptRepository
{
    public ImportReceiptRepository(AppDbContext context) : base(context) { }
}