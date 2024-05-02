using System.Text.Json;
using EventStore.Client;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public abstract class EventStoreDBTest: IDisposable
{
    protected readonly EventStoreClient EventStore = new(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));

    protected Task AppendEvents(string streamName, IEnumerable<object> events, CancellationToken ct)
    {
        // TODO: Fill append events logic here.
        return EventStore.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            events.Select(@event =>
                new EventData(
                    Uuid.NewUuid(),
                    @event.GetType().FullName!,
                    JsonSerializer.SerializeToUtf8Bytes(@event)
                )
            ), cancellationToken: ct);
    }

    public virtual void Dispose()
    {
        EventStore.Dispose();
    }
}
