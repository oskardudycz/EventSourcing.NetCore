namespace ECommerce.Domain.Core.Repositories;

public static class QueryableExtensions
{
    public static IQueryable<TEntity> GetPage<TEntity>(
        this IQueryable<TEntity> query,
        int pageNumber = 1,
        int pageSize = 20
    ) =>
        query
            .Skip(pageNumber * pageSize)
            .Take(pageSize);
}
