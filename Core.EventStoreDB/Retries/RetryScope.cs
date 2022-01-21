using System;
using System.Collections.Generic;
using System.Linq;
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

    public static Task<List<ResolvedEvent>> ReadAllStreamsEventsWithRetry(
        this EventStoreClient eventStore,
        string streamName,
        CancellationToken cancellationToken
    ) =>
        eventStore.ExecuteAsync(
            async (es, ct) =>
            {
                await using var readResult = es.ReadStreamAsync(
                    Direction.Forwards,
                    streamName,
                    StreamPosition.Start,
                    cancellationToken: ct
                );

                if(await readResult.ReadState != ReadState.Ok)
                    throw new ArgumentOutOfRangeException(
                        nameof(streamName), $"Stream '{streamName}' was not found"
                    );

                return await readResult
                    .ToListAsync(ct);
            },
            cancellationToken);
}
