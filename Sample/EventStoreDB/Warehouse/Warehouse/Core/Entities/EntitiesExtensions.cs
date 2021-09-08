using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Warehouse.Core.Entities
{
    public static class EntitiesExtensions
    {
        public static async ValueTask AddAndSave<T>(this DbContext dbContext, T entity, CancellationToken ct)
            where T : notnull
        {
            await dbContext.AddAsync(entity, ct);
            await dbContext.SaveChangesAsync(ct);
        }

        public static ValueTask<T> Find<T, TId>(this DbContext dbContext, TId id, CancellationToken ct)
            where T : class where TId : notnull
            => dbContext.FindAsync<T>(new object[] {id}, ct);
    }
}
