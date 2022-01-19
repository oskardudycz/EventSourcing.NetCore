using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventStore.Client;
using Polly;
using Polly.Wrap;

namespace Core.EventStoreDB.Retries;

public static class RetryScope
{
    private static readonly AsyncPolicyWrap RetryPolicy
        = Policy
            .Handle<Exception>()
            .RetryAsync(7)
            .WrapAsync(Policy.TimeoutAsync(5));

    public static Task ExecuteAsync(
        this EventStoreClient eventStore,
        Func<EventStoreClient, CancellationToken, Task> action,
        CancellationToken cancellationToken
    ) =>
        RetryPolicy.ExecuteAsync(ct => action(eventStore, ct), cancellationToken);

    public static Task<TResult> ExecuteAsync<TResult>(
        this EventStoreClient eventStore,
        Func<EventStoreClient, CancellationToken, Task<TResult>> action,
        CancellationToken cancellationToken
    ) =>
        RetryPolicy.ExecuteAsync(ct => action(eventStore, ct), cancellationToken);

    public static Task<IWriteResult> AppendWithRetry(
        this EventStoreClient eventStore,
        string streamName,
        StreamRevision expectedRevision,
        IEnumerable<EventData> eventData,
        CancellationToken cancellationToken
    ) =>
        eventStore.ExecuteAsync(
            (es, ct) => es.AppendToStreamAsync(streamName, expectedRevision, eventData, cancellationToken: ct),
            cancellationToken);
}
