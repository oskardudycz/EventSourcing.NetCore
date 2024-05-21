namespace Core.EventStoreDB.Subscriptions;

public class EventStoreDBSubscriptioToAllCoordinator(IDictionary<string, EventStoreDBSubscriptionToAll> subscriptions)
{
    public async Task SubscribeToAll(CancellationToken ct)
    {
        // see: https://github.com/dotnet/runtime/issues/36063
        await Task.Yield();

        await Task.WhenAll(subscriptions.Values.Select(s => s.SubscribeToAll(ct))).ConfigureAwait(false);
    }
}
