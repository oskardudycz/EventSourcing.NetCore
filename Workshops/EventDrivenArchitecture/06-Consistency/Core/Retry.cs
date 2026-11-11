using Polly;
using Polly.Retry;

namespace Consistency.Core;

public static class Retry
{
    private const int MaxRetries = 9;

    private static readonly AsyncRetryPolicy retryPolicy = Policy
        .Handle<ConcurrencyException>()
        .RetryAsync(MaxRetries);

    public static Task<T> OnConflict<T>(Func<CancellationToken, ValueTask<T>> handle, CancellationToken token = default) =>
        retryPolicy.ExecuteAsync(async ct => await handle(ct), token);
}
