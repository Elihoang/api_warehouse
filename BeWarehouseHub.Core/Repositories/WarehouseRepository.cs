using BeWarehouseHub.Core.Configurations;
using BeWarehouseHub.Domain.Interfaces;
using BeWarehouseHub.Domain.Models;

namespace BeWarehouseHub.Core.Repositories;

public class WarehouseRepository : BaseRepository<Warehouse>, IWarehouseRepository
{
    public WarehouseRepository(AppDbContext context) : base(context)
    {
    }
}