using Elastic.Clients.Elasticsearch;
using Marten;
using Marten.Events;
using Marten.Events.Daemon;

namespace MartenMeetsElastic.Tests;

public abstract class MartenMeetsElasticTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected IDocumentSession DocumentSession = default!;
    protected ElasticsearchClient elasticClient;

    protected IProjectionDaemon daemon = default!;

    protected MartenMeetsElasticTest()
    {
        elasticClient = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri("http://localhost:9200/")));

        var options = new StoreOptions();
        options.Connection(
            "PORT = 5432; HOST = localhost; TIMEOUT = 15; POOLING = True; DATABASE = 'postgres'; PASSWORD = 'Password12!'; USER ID = 'postgres'");
        options.UseNewtonsoftForSerialization(nonPublicMembersStorage: NonPublicMembersStorage.All);
        options.DatabaseSchemaName = options.Events.DatabaseSchemaName = "MartenMeetsElastic";
        options.Events.StreamIdentity = StreamIdentity.AsString;

        Options(options);

        documentStore = new DocumentStore(options);
        ReOpenSession();
    }

    protected virtual void Options(StoreOptions options) { }

    protected void ReOpenSession()
    {
        DocumentSession = documentStore.LightweightSession();
    }

    internal async Task<IProjectionDaemon> StartDaemon()
    {
        daemon = await documentStore.BuildProjectionDaemonAsync();

        await daemon.StartAllAsync();

        return daemon;
    }

    protected Task AppendEvents(string streamId, params object[] events)
    {
        DocumentSession.Events.Append(
            streamId,
            events
        );
        return DocumentSession.SaveChangesAsync();
    }

    public virtual void Dispose()
    {
        DocumentSession.Dispose();
        documentStore.Dispose();
    }
}
