using Weasel.Core;
using Marten;
using IntroductionToEventSourcing.Tools;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public abstract class MartenTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected readonly IDocumentSession DocumentSession;

    protected MartenTest(string schemaName)
    {
        var options = new StoreOptions();
        options.Connection(TestDatabases.PostgresConnectionString);
        options.UseSystemTextJsonForSerialization();
        options.DatabaseSchemaName = options.Events.DatabaseSchemaName = schemaName;

        documentStore = new DocumentStore(options);
        DocumentSession = documentStore.LightweightSession();
    }

    protected Task AppendEvents(Guid streamId, object[] events, CancellationToken ct)
    {
        DocumentSession.Events.Append(
            streamId,
            events
        );
        return DocumentSession.SaveChangesAsync(ct);
    }

    public virtual void Dispose()
    {
        DocumentSession.Dispose();
        documentStore.Dispose();
    }
}
