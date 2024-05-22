using Core.Events;
using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.Extensions;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Wrap;

namespace Core.EventStoreDB.Subscriptions;
using static ISubscriptionCheckpointRepository;

public class EventStoreDBSubscriptionToAllOptions
{
    public required string SubscriptionId { get; init; }

    public SubscriptionFilterOptions FilterOptions { get; set; } =
        new(EventTypeFilter.ExcludeSystemEvents());

    public Action<EventStoreClientOperationOptions>? ConfigureOperation { get; set; }
    public UserCredentials? Credentials { get; set; }
    public bool ResolveLinkTos { get; set; }
    public bool IgnoreDeserializationErrors { get; set; } = true;

    public int BatchSize { get; set; } = 1;
    public int BatchDeadline { get; set; } = 50;
}

public class EventStoreDBSubscriptionToAll(
    EventStoreClient eventStoreClient,
    IServiceScopeFactory serviceScopeFactory,
    ILogger<EventStoreDBSubscriptionToAll> logger
)
{
    public enum ProcessingStatus
    {
        NotStarted,
        Starting,
        Started,
        Paused,
        Errored,
        Stopped
    }

    public record EventBatch(string SubscriptionId, ResolvedEvent[] Events);

    public EventStoreDBSubscriptionToAllOptions Options { get; set; } = default!;

    public Func<IServiceProvider, IEventBatchHandler[]> GetHandlers { get; set; } = default!;

    public ProcessingStatus Status = ProcessingStatus.NotStarted;

    private string SubscriptionId => Options.SubscriptionId;

    public async Task SubscribeToAll(Checkpoint checkpoint, CancellationToken ct)
    {
        Status = ProcessingStatus.Starting;

        logger.LogInformation("Subscription to all '{SubscriptionId}'", Options.SubscriptionId);

        await RetryPolicy.ExecuteAsync(token =>
                OnSubscribe(checkpoint, token), ct
        ).ConfigureAwait(false);
    }

    private async Task OnSubscribe(Checkpoint checkpoint, CancellationToken ct)
    {
        var subscription = eventStoreClient.SubscribeToAll(
            checkpoint != Checkpoint.None ? FromAll.After(checkpoint) : FromAll.Start,
            Options.ResolveLinkTos,
            Options.FilterOptions,
            Options.Credentials,
            ct
        );

        Status = ProcessingStatus.Started;

        logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);

        await foreach(var events in subscription.Batch(Options.BatchSize, Options.BatchDeadline, ct))
        {
            var batch = new EventBatch(Options.SubscriptionId, events.ToArray());
            var result = await ProcessBatch(batch, checkpoint, ct).ConfigureAwait(false);

            if (result is StoreResult.Success success)
            {
                checkpoint = success.Checkpoint;
            }
        }
    }

    private async Task<StoreResult> ProcessBatch(EventBatch batch, Checkpoint lastCheckpoint, CancellationToken ct)
    {
        try
        {
            await using var scope = serviceScopeFactory.CreateAsyncScope();
            var checkpointer = scope.ServiceProvider.GetRequiredService<IEventsBatchCheckpointer>();

            return await checkpointer.Process(
                batch.Events,
                lastCheckpoint,
                new BatchProcessingOptions(
                    batch.SubscriptionId,
                    Options.IgnoreDeserializationErrors,
                    GetHandlers(scope.ServiceProvider)
                ),
                ct
            ).ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            logger.LogError(exc, "Error while handling batch");
            throw;
        }
    }

    private AsyncPolicyWrap RetryPolicy
    {
        get
        {
            var generalPolicy = Policy.Handle<Exception>(ex => !IsCancelledByUser(ex))
                .WaitAndRetryForeverAsync(
                    sleepDurationProvider: _ =>
                        TimeSpan.FromMilliseconds(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000)),
                    onRetry: (exception, _, _) =>
                        logger.LogWarning("Subscription was dropped: {Exception}", exception)
                );

            var fallbackPolicy = Policy.Handle<OperationCanceledException>()
                .Or<RpcException>(IsCancelledByUser)
                .FallbackAsync(_ =>
                    {
                        logger.LogWarning("Subscription to all '{SubscriptionId}' dropped by client", SubscriptionId);
                        return Task.CompletedTask;
                    }
                );

            return Policy.WrapAsync(generalPolicy, fallbackPolicy);
        }
    }

    private static bool IsCancelledByUser(RpcException rpcException) =>
        rpcException.StatusCode == StatusCode.Cancelled
        || rpcException.InnerException is ObjectDisposedException;

    private static bool IsCancelledByUser(Exception exception) =>
        exception is OperationCanceledException
        || exception is RpcException rpcException && IsCancelledByUser(rpcException);

}
