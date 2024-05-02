using Microsoft.EntityFrameworkCore;

namespace SlimDownYourAggregate.Core.EF;

public class EFRepository<TDbContext, T>(TDbContext dbContext)
    where TDbContext : DbContext
    where T : class
{
    public ValueTask<T?> FindAsync(string id, CancellationToken ct) =>
        dbContext.FindAsync<T>(id, ct);

    public async Task AddAsync(T aggregate, CancellationToken ct)
    {
        dbContext.Set<T>().Add(aggregate);

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task UpdateAsync(T aggregate, CancellationToken ct)
    {
        dbContext.Set<T>().Update(aggregate);

        await dbContext.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(T aggregate, CancellationToken ct)
    {
        dbContext.Set<T>().Remove(aggregate);

        await dbContext.SaveChangesAsync(ct);
    }
}

public static class EFRepositoryExtensions
{
    public static async Task GetAndUpdateAsync<TDbContext, T>(
        this EFRepository<TDbContext, T> repository,
        string id,
        Action<T> handle,
        CancellationToken ct
    ) where TDbContext : DbContext where T : class
    {
        var entity = await repository.FindAsync(id, ct);

        if (entity == null)
            throw new InvalidOperationException($"{nameof(T)} with id '{id}' was not found!");

        handle(entity);

        await repository.UpdateAsync(entity, ct);
    }
}
