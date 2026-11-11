using Polly;
using Polly.Retry;

namespace Consistency.Core;

public static class Retry
{
    private const int MaxRetries = 9;

    private static readonly AsyncRetryPolicy untilSucceedsPolicy = Policy
        .Handle<TimeoutException>()
        .RetryAsync(MaxRetries);

    private static readonly AsyncRetryPolicy onConflictPolicy = Policy
        .Handle<ConcurrencyException>()
        .RetryAsync(MaxRetries);

    public static Task UntilSucceeds(Func<CancellationToken, ValueTask> handle, CancellationToken token = default) =>
        untilSucceedsPolicy.ExecuteAsync(async ct => await handle(ct), token);

    public static Task<T> UntilSucceeds<T>(Func<CancellationToken, ValueTask<T>> handle, CancellationToken token = default) =>
        untilSucceedsPolicy.ExecuteAsync(async ct => await handle(ct), token);

    public static Task OnConflict(Func<CancellationToken, ValueTask> handle, CancellationToken token = default) =>
        onConflictPolicy.ExecuteAsync(async ct => await handle(ct), token);

    public static Task<T> OnConflict<T>(Func<CancellationToken, ValueTask<T>> handle, CancellationToken token = default) =>
        onConflictPolicy.ExecuteAsync(async ct => await handle(ct), token);
}
