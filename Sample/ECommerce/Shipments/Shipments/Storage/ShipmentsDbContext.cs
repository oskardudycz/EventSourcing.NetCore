using Microsoft.EntityFrameworkCore;
using Shipments.Packages;

namespace Shipments.Storage;

internal class ShipmentsDbContext(DbContextOptions<ShipmentsDbContext> options): DbContext(options)
{
    public DbSet<Package> Packages { get; set; } = null!;
}
