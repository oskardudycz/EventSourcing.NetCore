using System.Data;
using FluentAssertions;
using Marten.Events;
using Npgsql;
using Xunit;

namespace Marten.Integration.Tests.TestsInfrastructure;

[Collection("Marten")]
public abstract class MartenTest: IDisposable
{
    protected IDocumentSession Session = default!;

    protected IEventStore EventStore
    {
        get { return Session!.Events; }
    }

    protected readonly string SchemaName = "sch" + Guid.NewGuid().ToString().Replace("-", string.Empty);

    protected MartenTest() : this(true)
    {
    }

    protected MartenTest(bool shouldCreateSession)
    {
        if (shouldCreateSession)
            Session = CreateSession();
    }

    protected virtual IDocumentSession CreateSession(Action<StoreOptions>? storeOptions = null) =>
        DocumentSessionProvider.Get(SchemaName, storeOptions);

    public void Dispose()
    {
        Session?.Dispose();

        var sql = $"DROP SCHEMA {SchemaName} CASCADE;";

        using var conn = new NpgsqlConnection(Settings.ConnectionString);
        conn.Open();

        using var tran = conn.BeginTransaction(IsolationLevel.ReadCommitted);
        var command = conn.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();

        tran.Commit();
    }

    protected Task SaveChangesAsync() =>
        Session.SaveChangesAsync();

    protected async Task SaveChangesAsyncShouldFailWith<TException>() where TException: Exception
    {
        var saveChanges = SaveChangesAsync;
        await saveChanges.Should().ThrowAsync<TException>();

        Session = CreateSession();
    }
}
