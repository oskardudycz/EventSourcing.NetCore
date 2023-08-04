using System.Data;
using DotNet.Testcontainers.Containers;
using FluentAssertions;
using Marten.Events;
using Npgsql;
using Testcontainers.PostgreSql;
using Xunit;

namespace Marten.Integration.Tests.TestsInfrastructure;

public abstract class MartenTest: IAsyncLifetime
{
    protected IDocumentSession Session => session ?? throw new InvalidOperationException("Session is not initialised!");


    private IDocumentSession? session = default!;

    protected string ConnectionString => postgreSqlContainer.GetConnectionString();

    private readonly PostgreSqlContainer postgreSqlContainer = new PostgreSqlBuilder()
        .Build();


    protected IEventStore EventStore
    {
        get { return Session!.Events; }
    }

    protected readonly string SchemaName;
    private readonly bool shouldCreateSession;

    protected MartenTest(): this(true, true)
    {
    }

    protected MartenTest(bool shouldCreateSession = true, bool useRandomSchema = true)
    {
        this.shouldCreateSession = shouldCreateSession;

        SchemaName = useRandomSchema ? "sch" + Guid.NewGuid().ToString().Replace("-", string.Empty) : "EventStore";
    }

    protected virtual IDocumentSession CreateSession(Action<StoreOptions>? storeOptions = null)
    {
        if (postgreSqlContainer.State != TestcontainersStates.Running)
            throw new InvalidOperationException($"Container is not started! ({postgreSqlContainer.State})");

        return DocumentSessionProvider.Get(
            postgreSqlContainer.GetConnectionString(),
            SchemaName,
            storeOptions
        );
    }

    protected Task SaveChangesAsync() =>
        Session.SaveChangesAsync();

    protected async Task SaveChangesAsyncShouldFailWith<TException>() where TException : Exception
    {
        var saveChanges = SaveChangesAsync;
        await saveChanges.Should().ThrowAsync<TException>();

        session = CreateSession();
    }

    public virtual async Task InitializeAsync()
    {
        await postgreSqlContainer.StartAsync();

        if (shouldCreateSession)
            session = CreateSession();
    }

    public virtual async Task DisposeAsync()
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

        await postgreSqlContainer.DisposeAsync();
    }
}
