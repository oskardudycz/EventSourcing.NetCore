using Open.ChannelExtensions;

namespace Core.Extensions;

public static class AsyncEnumerableExtensions
{
    public static IAsyncEnumerable<List<T>> Batch<T>(
        this IAsyncEnumerable<T> enumerable,
        int batchSize,
        TimeSpan deadline,
        CancellationToken ct
    ) =>
        enumerable
            .ToChannel(cancellationToken: ct)
            .Batch(batchSize)
            .WithTimeout(deadline)
            .AsAsyncEnumerable(cancellationToken: ct);

}
