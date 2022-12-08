using Core.Aggregates;
using Core.Marten.OptimisticConcurrency;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.Repository;

public class MartenRepositoryWithETagDecorator<T>: IMartenRepository<T>
    where T : class, IAggregate
{
    private readonly IMartenRepository<T> inner;
    private readonly Func<long?> getExpectedVersion;
    private readonly Action<long> setNextExpectedVersion;

    public MartenRepositoryWithETagDecorator(
        IMartenRepository<T> inner,
        Func<long?> getExpectedVersion,
        Action<long> setNextExpectedVersion
    )
    {
        this.inner = inner;
        this.getExpectedVersion = getExpectedVersion;
        this.setNextExpectedVersion = setNextExpectedVersion;
    }

    public Task<T?> Find(Guid id, CancellationToken cancellationToken) =>
        inner.Find(id, cancellationToken);

    public async Task<long> Add(T aggregate, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Add(
            aggregate,
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }

    public async Task<long> Update(T aggregate, long? expectedVersion = null,
        CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Update(
            aggregate,
            expectedVersion ?? getExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }

    public async Task<long> Delete(T aggregate, long? expectedVersion = null, CancellationToken cancellationToken = default)
    {
        var nextExpectedVersion = await inner.Delete(
            aggregate,
            expectedVersion ?? getExpectedVersion(),
            cancellationToken
        ).ConfigureAwait(true);

        setNextExpectedVersion(nextExpectedVersion);

        return nextExpectedVersion;
    }
}

public static class MartenAppendScopeExtensions
{
    public static IServiceCollection AddMartenAppendScope(this IServiceCollection services) =>
        services
            .AddScoped<MartenExpectedStreamVersionProvider, MartenExpectedStreamVersionProvider>()
            .AddScoped<MartenNextStreamVersionProvider, MartenNextStreamVersionProvider>();
}
