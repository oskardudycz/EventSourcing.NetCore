using System.Threading.Channels;
using Core.Events;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.Extensions;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using Open.ChannelExtensions;
using Polly;
using Polly.Wrap;
using EventTypeFilter = EventStore.Client.EventTypeFilter;

namespace Core.EventStoreDB.Subscriptions;

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

    public EventStoreDBSubscriptionToAllOptions Options { get; set; } = default!;

    public Func<IServiceProvider, IEventBatchHandler[]> GetHandlers { get; set; } = default!;

    public ProcessingStatus Status = ProcessingStatus.NotStarted;

    private string SubscriptionId => Options.SubscriptionId;

    public async Task SubscribeToAll(Checkpoint checkpoint, ChannelWriter<EventBatch> cw, CancellationToken ct)
    {
        Status = ProcessingStatus.Starting;

        logger.LogInformation("Subscription to all '{SubscriptionId}'", Options.SubscriptionId);

        await RetryPolicy.ExecuteAsync(token =>
                OnSubscribe(checkpoint, cw, token), ct
        ).ConfigureAwait(false);
    }

    private async Task OnSubscribe(Checkpoint checkpoint, ChannelWriter<EventBatch> cw, CancellationToken ct)
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

        await subscription.Pipe<ResolvedEvent, EventBatch>(
            cw,
            events => new EventBatch(Options.SubscriptionId, events.ToArray()),
            Options.BatchSize,
            Options.BatchDeadline,
            ct
        ).ConfigureAwait(false);
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
