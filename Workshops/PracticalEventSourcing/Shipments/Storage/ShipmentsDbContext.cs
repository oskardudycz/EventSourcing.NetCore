using Microsoft.EntityFrameworkCore;
using Shipments.Packages;

namespace Shipments.Storage
{
    public class ShipmentsDbContext: DbContext
    {
        public ShipmentsDbContext(DbContextOptions<ShipmentsDbContext> options)
            : base(options)
        {
        }

        public DbSet<Package> Packages { get; set; }
    }
}
