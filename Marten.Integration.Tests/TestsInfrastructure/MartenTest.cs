using System.Data;
using Shouldly;
using Marten.Events;
using Npgsql;
using Xunit;

namespace Marten.Integration.Tests.TestsInfrastructure;

[CollectionDefinition("MartenIntegrationTests", DisableParallelization = false)]
public class MartenCollection;

[Collection("MartenIntegrationTests")]
public abstract class MartenTest(
    string connectionString,
    bool shouldCreateSession = true,
    bool useRandomSchema = true)
    : IAsyncLifetime
{
    protected IDocumentSession Session => session ?? throw new InvalidOperationException("Session is not initialised!");

    private IDocumentSession? session;

    protected string ConnectionString => connectionString;

    protected IEventStoreOperations EventStore => Session.Events;
    protected static string GenerateRandomId() => Guid.CreateVersion7().ToString();

    protected readonly string SchemaName = useRandomSchema ? "sch" + Guid.CreateVersion7().ToString("N") : "EventStore";

    protected virtual IDocumentSession CreateSession(Action<StoreOptions>? storeOptions = null)
    {
        return DocumentSessionProvider.Get(
            connectionString,
            SchemaName,
            storeOptions
        );
    }

    protected Task SaveChangesAsync() =>
        Session.SaveChangesAsync();

    protected async Task SaveChangesAsyncShouldFailWith<TException>() where TException : Exception
    {
        var saveChanges = SaveChangesAsync;
        await saveChanges.ShouldThrowAsync<TException>();

        session = CreateSession();
    }

    public virtual ValueTask InitializeAsync()
    {
        if (shouldCreateSession)
            session = CreateSession();

        return ValueTask.CompletedTask;
    }

    public virtual async ValueTask DisposeAsync()
    {
        if (session != null)
            await session.DisposeAsync();

        var sql = $"DROP SCHEMA IF EXISTS {SchemaName} CASCADE;";

        await using var conn = new NpgsqlConnection(ConnectionString);
        conn.Open();

        await using var tran = await conn.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        var command = conn.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();

        await tran.CommitAsync();
    }
}
