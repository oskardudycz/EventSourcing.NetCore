namespace Core.Queries;

public interface IQueryBus
{
    Task<TResponse> Query<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : notnull;
}
