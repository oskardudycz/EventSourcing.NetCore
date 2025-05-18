using Polly;
using Polly.Retry;

namespace Consistency.Sagas.Version1_Aggregates.Core;

public class Retry
{
    public static int RetriesCount;

    private static readonly AsyncRetryPolicy retryPolicy = Policy
        .Handle<TimeoutException>()
        .WaitAndRetryForeverAsync(
            (_, _) =>
            {
                RetriesCount++;

                return TimeSpan.FromMilliseconds(1);
            });

    public Task UntilSucceeds(Func<CancellationToken, ValueTask> handle, CancellationToken token = default) =>
        retryPolicy.ExecuteAsync(async ct => await handle(ct), token);
}
