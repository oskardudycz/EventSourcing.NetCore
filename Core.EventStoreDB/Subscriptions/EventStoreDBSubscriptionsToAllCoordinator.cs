using Core.EventStoreDB.Subscriptions.Checkpoints;
using EventStore.Client;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace Core.EventStoreDB.Subscriptions;

public class EventStoreDBSubscriptionsToAllCoordinator(
    IDictionary<string, EventStoreDBSubscriptionToAll> subscriptions,
    ISubscriptionStoreSetup checkpointStoreSetup,
    IServiceScopeFactory serviceScopeFactory
)
{
    public async Task SubscribeToAll(CancellationToken ct)
    {
        await checkpointStoreSetup.EnsureStoreExists(ct).ConfigureAwait(false);

        var tasks = subscriptions.Select(s => Task.Run(async () =>
        {
            var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var token = cts.Token;

            var checkpoint = await LoadCheckpoint(s.Key, token).ConfigureAwait(false);

            await s.Value.SubscribeToAll(checkpoint, ct).ConfigureAwait(false);
        }, ct)).ToArray();

        await Task.WhenAll(tasks).ConfigureAwait(false);
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
