using EventStore.Client;

namespace IntroductionToEventSourcing.BusinessLogic.Tools;

public abstract class EventStoreDBTest: IDisposable
{
    protected readonly EventStoreClient EventStore;

    protected EventStoreDBTest()
    {
        EventStore =
            new EventStoreClient(EventStoreClientSettings.Create("esdb://localhost:2113?tls=false"));
    }

    public virtual void Dispose()
    {
        EventStore.Dispose();
    }
}
