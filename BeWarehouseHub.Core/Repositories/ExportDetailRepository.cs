using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BeWarehouseHub.Core.Repositories;

public class ExportDetailRepository : BaseRepository<ExportDetail>, IExportDetailRepository
{
    public ExportDetailRepository(AppDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<ExportDetail>> GetByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ExportDetails
            .Include(e => e.Product)
            .Include(e => e.Stock)
                .ThenInclude(s => s.Warehouse)
            .Where(e => e.DateExport >= startDate && e.DateExport <= endDate)
            .ToListAsync();
    }
}
