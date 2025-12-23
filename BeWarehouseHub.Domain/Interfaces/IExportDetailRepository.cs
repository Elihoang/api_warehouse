using BeWarehouseHub.Domain.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeWarehouseHub.Domain.Interfaces;

public interface IExportDetailRepository : IRepository<ExportDetail>
{
    Task<IEnumerable<ExportDetail>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
}