using Marten;

namespace IntroductionToEventSourcing.OptimisticConcurrency.Tools;

public abstract class MartenTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected IDocumentSession DocumentSession = default!;

    protected MartenTest()
    {
        var options = new StoreOptions();
        options.Connection(
            "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");
        options.UseSystemTextJsonForSerialization();
        options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "IntroductionToEventSourcing";

        documentStore = new DocumentStore(options);
        ReOpenSession();
    }

    protected void ReOpenSession()
    {
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
