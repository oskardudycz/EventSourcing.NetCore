using Core.Testing.Fixtures;
using Weasel.Core;
using Elastic.Clients.Elasticsearch;
using Marten;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using Marten.Events.Daemon;

namespace MartenMeetsElastic.Tests;

public abstract class MartenMeetsElasticTest: IDisposable
{
    private readonly DocumentStore documentStore;
    protected IDocumentSession DocumentSession = null!;
    protected ElasticsearchClient elasticClient;

    protected IProjectionDaemon daemon = null!;

    protected MartenMeetsElasticTest(PostgresContainerFixture postgres, ElasticsearchContainerFixture elasticsearch)
    {
        elasticClient = new ElasticsearchClient(new ElasticsearchClientSettings(new Uri(elasticsearch.Url)));

        var options = new StoreOptions();
        options.Connection(postgres.ConnectionString);
        options.UseSystemTextJsonForSerialization();
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
