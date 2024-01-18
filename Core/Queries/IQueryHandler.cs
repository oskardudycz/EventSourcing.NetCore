namespace Core.Queries;

public interface IQueryHandler<in TQuery, TResponse>
    where TQuery : notnull
{
    Task<TResponse> Handle(TQuery request, CancellationToken cancellationToken);
}
