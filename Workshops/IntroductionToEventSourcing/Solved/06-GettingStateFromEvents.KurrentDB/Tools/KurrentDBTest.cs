using System.Text.Json;
using KurrentDB.Client;
using IntroductionToEventSourcing.Tools;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public abstract class KurrentDBTest: IDisposable
{
    protected readonly KurrentDBClient EventStore = new(KurrentDBClientSettings.Create(TestDatabases.KurrentDBConnectionString));

    protected Task AppendEvents(string streamName, IEnumerable<object> events, CancellationToken ct) =>
        // TODO: Fill append events logic here.
        EventStore.AppendToStreamAsync(
            streamName,
            StreamState.Any,
            events.Select(@event =>
                new EventData(
                    Uuid.NewUuid(),
                    @event.GetType().FullName!,
                    JsonSerializer.SerializeToUtf8Bytes(@event)
                )
            ), cancellationToken: ct);

    public virtual void Dispose() =>
        EventStore.Dispose();
}
