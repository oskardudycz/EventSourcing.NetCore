using System.Threading.Channels;
using Core.EventStoreDB.Subscriptions.Batch;
using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Core.EventStoreDB.Subscriptions;

public class SubscriptionInfo
{
    public required EventStoreDBSubscriptionToAll Subscription { get; set; }
    public required Checkpoint LastCheckpoint { get; set; }
};

public class EventStoreDBSubscriptioToAllCoordinator
{
    private readonly IDictionary<string, SubscriptionInfo> subscriptions;
    private readonly IServiceScopeFactory serviceScopeFactory;

    private readonly Channel<EventBatch> events = Channel.CreateBounded<EventBatch>(
        new BoundedChannelOptions(1)
        {
            SingleWriter = false,
            SingleReader = true,
            AllowSynchronousContinuations = false,
            FullMode = BoundedChannelFullMode.Wait
        }
    );

    public EventStoreDBSubscriptioToAllCoordinator(IDictionary<string, EventStoreDBSubscriptionToAll> subscriptions,
        IServiceScopeFactory serviceScopeFactory)
    {
        this.subscriptions =
            subscriptions.ToDictionary(ks => ks.Key,
                vs => new SubscriptionInfo { Subscription = vs.Value, LastCheckpoint = Checkpoint.None }
            );
        this.serviceScopeFactory = serviceScopeFactory;
    }

    public ChannelReader<EventBatch> Reader => events.Reader;
    public ChannelWriter<EventBatch> Writer => events.Writer;

    public async Task SubscribeToAll(CancellationToken ct)
    {
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        var tasks = subscriptions.Select(s => Task.Run(async () =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var token = cts.Token;

            var checkpoint = await LoadCheckpoint(s.Key, token).ConfigureAwait(false);
            subscriptions[s.Key].LastCheckpoint = checkpoint;

            await s.Value.Subscription.SubscribeToAll(checkpoint, Writer, token).ConfigureAwait(false);
        }, ct)).ToList();
        var process = ProcessMessages(ct);

        await Task.WhenAll([process, ..tasks]).ConfigureAwait(false);
    }

    public async Task ProcessMessages(CancellationToken ct)
    {
        while (!Reader.Completion.IsCompleted && await Reader.WaitToReadAsync(ct).ConfigureAwait(false))
        {
            if (!Reader.TryPeek(out var batch)) continue;

            try
            {
                await ProcessBatch(ct, batch).ConfigureAwait(false);
            }
            catch (Exception exc)
            {
                Console.WriteLine(exc);
            }
        }
    }

    private async Task ProcessBatch(CancellationToken ct, EventBatch batch)
    {
        using var scope = serviceScopeFactory.CreateScope();
        var checkpointer = scope.ServiceProvider.GetRequiredService<IEventsBatchCheckpointer>();

        var subscriptionInfo = subscriptions[batch.SubscriptionId];

        var result = await checkpointer.Process(
                batch.Events,
                subscriptionInfo.LastCheckpoint,
                new BatchProcessingOptions(
                    batch.SubscriptionId,
                    subscriptionInfo.Subscription.Options.IgnoreDeserializationErrors,
                    subscriptionInfo.Subscription.GetHandlers(scope.ServiceProvider)
                ),
                ct
            )
            .ConfigureAwait(false);


        if (result is ISubscriptionCheckpointRepository.StoreResult.Success success)
        {
            subscriptionInfo.LastCheckpoint = success.Checkpoint;
            Reader.TryRead(out _);
        }
    }

    private Task<Checkpoint> LoadCheckpoint(string subscriptionId, CancellationToken token) =>
        Policy.Handle<Exception>().RetryAsync(3)
            .ExecuteAsync<Checkpoint>(async ct =>
            {
                using var scope = serviceScopeFactory.CreateScope();
                return await scope.ServiceProvider.GetRequiredService<ISubscriptionCheckpointRepository>()
                    .Load(subscriptionId, ct).ConfigureAwait(false);
            }, token);
}

public record EventBatch(string SubscriptionId, ResolvedEvent[] Events);
