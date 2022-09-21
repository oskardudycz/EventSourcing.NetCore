using Microsoft.EntityFrameworkCore;

namespace ECommerce.Domain.Core.Repositories;

public static class DbContextExtensions
{
    public static ValueTask<TEntity?> FindById<TEntity>(this DbContext dbContext, Guid id, CancellationToken ct)
        where TEntity : class =>
        dbContext.Set<TEntity>().FindAsync(
            new object?[] { id },
            cancellationToken: ct
        );

    public static Task AddAndSaveChanges<TEntity>(this DbContext dbContext, TEntity entity, CancellationToken ct)
        where TEntity : class =>
        dbContext.HandleAndSaveChanges((db, _) =>
        {
            db.Add(entity);
            return ValueTask.CompletedTask;
        }, ct);

    public static Task UpdateAndSaveChanges<TEntity>(this DbContext dbContext, TEntity entity, CancellationToken ct)
        where TEntity : class =>
        dbContext.HandleAndSaveChanges((db, _) =>
        {
            db.Update(entity);
            return ValueTask.CompletedTask;
        }, ct);

    public static Task UpdateAndSaveChanges<TEntity>(this DbContext dbContext, Guid id, Func<TEntity, TEntity> handle,
        CancellationToken ct)
        where TEntity : class =>
        dbContext.HandleAndSaveChanges(async (db, _) =>
        {
            var entity = await dbContext.FindById<TEntity>(id, ct);

            if (entity == null)
                throw new ArgumentOutOfRangeException(nameof(id));

            var updatedEntity = handle(entity);
            await db.UpdateAndSaveChanges(updatedEntity, ct);
        }, ct);

    public static Task DeleteAndSaveChanges<TEntity>(this DbContext dbContext, Guid id, CancellationToken ct)
        where TEntity : class =>
        dbContext.HandleAndSaveChanges(async (db, _) =>
        {
            var entity = await dbContext.FindById<TEntity>(id, ct);

            if (entity == null)
                throw new ArgumentOutOfRangeException(nameof(id));

            db.Remove(entity);
        }, ct);

    public static async Task HandleAndSaveChanges(this DbContext dbContext,
        Func<DbContext, CancellationToken, ValueTask> handle, CancellationToken ct)
    {
        await handle(dbContext, ct);
        await dbContext.SaveChangesAsync(ct);
    }
}
