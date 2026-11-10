using Polly;
using Polly.Retry;

namespace Consistency.Core;

public static class Retry
{
    private const int MaxRetries = 9;

    private static readonly AsyncRetryPolicy retryPolicy = Policy
        .Handle<TimeoutException>()
        .RetryAsync(MaxRetries);

    public static Task UntilSucceeds(Func<CancellationToken, ValueTask> handle, CancellationToken token = default) =>
        retryPolicy.ExecuteAsync(async ct => await handle(ct), token);
}
