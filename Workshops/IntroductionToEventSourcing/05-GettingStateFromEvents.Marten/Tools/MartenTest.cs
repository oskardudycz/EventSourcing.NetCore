using Marten;
using MysticMind.PostgresEmbed;

namespace IntroductionToEventSourcing.GettingStateFromEvents.Tools;

public abstract class MartenTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected readonly IDocumentSession DocumentSession;
    protected readonly PgServer PgServer;

    protected MartenTest()
    {
        PgServer = new PgServer("15.3.0");

        PgServer.Start();
        // using Npgsql to connect the server
        var connectionString =
            $"Server=localhost;Port={PgServer.PgPort};User Id=postgres;Password=test;Database=postgres";


        var options = new StoreOptions();
        options.Connection(connectionString);
        options.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);
        options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "IntroductionToEventSourcing";

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
        PgServer.Dispose();
    }
}
