using System.Threading.Channels;
using Core.Events;
using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.Extensions;
using EventStore.Client;
using Grpc.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using EventTypeFilter = EventStore.Client.EventTypeFilter;

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
}

public class EventStoreDBSubscriptionToAll(
    EventStoreClient eventStoreClient,
    ISubscriptionStoreSetup storeSetup,
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
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        logger.LogInformation("Subscription to all '{SubscriptionId}'", Options.SubscriptionId);

        try
        {
            await storeSetup.EnsureStoreExists(ct).ConfigureAwait(false);

            var subscription = eventStoreClient.SubscribeToAll(
                checkpoint != Checkpoint.None ? FromAll.After(checkpoint) : FromAll.Start,
                Options.ResolveLinkTos,
                Options.FilterOptions,
                Options.Credentials,
                ct
            );

            Status = ProcessingStatus.Started;

            logger.LogInformation("Subscription to all '{SubscriptionId}' started", SubscriptionId);

            await foreach (var @event in subscription)
                // TODO: Add proper batching here!
                // .BatchAsync(subscriptionOptions.BatchSize, TimeSpan.FromMilliseconds(100), ct)
                // .ConfigureAwait(false))
            {
                ResolvedEvent[] events = [@event];
                await cw.WriteAsync(new EventBatch(Options.SubscriptionId, events), ct)
                    .ConfigureAwait(false);
            }
        }
        catch (RpcException rpcException) when (rpcException is { StatusCode: StatusCode.Cancelled } ||
                                                rpcException.InnerException is ObjectDisposedException)
        {
            logger.LogWarning(
                "Subscription to all '{SubscriptionId}' dropped by client",
                SubscriptionId
            );
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning(
                "Subscription to all '{SubscriptionId}' dropped by client",
                SubscriptionId
            );
        }
        catch (Exception ex)
        {
            Status = ProcessingStatus.Errored;
            logger.LogWarning("Subscription was dropped: {Exception}", ex);

            // Sleep between reconnections to not flood the database or not kill the CPU with infinite loop
            // Randomness added to reduce the chance of multiple subscriptions trying to reconnect at the same time
            Thread.Sleep(1000 + new Random((int)DateTime.UtcNow.Ticks).Next(1000));

            await SubscribeToAll(checkpoint, cw, ct).ConfigureAwait(false);
        }
    }
}
