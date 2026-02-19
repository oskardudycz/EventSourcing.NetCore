using Dapper;
using FluentAssertions;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise03CreateAppendEventFunction: IAsyncLifetime
{
    public record User(
        string Name
    );

    public record UserCreated(
        string Name
    );

    private readonly NpgsqlConnection databaseConnection;
    private readonly PostgresSchemaProvider schemaProvider;
    private readonly EventStore eventStore;


    private const string AppendEventFunctionName = "append_event";

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise03CreateAppendEventFunction()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
        schemaProvider = new PostgresSchemaProvider(databaseConnection);

        // Create Event Store
        eventStore = new EventStore(databaseConnection);
    }

    [Fact]
    public void AppendEventFunction_ShouldBeCreated()
    {
        var appendFunctionExists = schemaProvider
            .FunctionExists(AppendEventFunctionName);

        appendFunctionExists.Should().BeTrue();
    }

    [Fact]
    public async Task AppendEventFunction_WhenStreamDoesNotExist_CreateNewStream_And_AppendNewEvent()
    {
        var streamId = Guid.CreateVersion7();
        var @event = new UserCreated("John Doe");

        var result = await eventStore.AppendEvent<User>(streamId, @event);

        result.Should().BeTrue();

        var wasStreamCreated = await databaseConnection.QuerySingleAsync<bool>(
            "select exists (select 1 from streams where id = @streamId)", new {streamId}
        );
        wasStreamCreated.Should().BeTrue();

        var wasEventAppended = await databaseConnection.QuerySingleAsync<bool>(
            "select exists (select 1 from events where stream_id = @streamId)", new {streamId}
        );
        wasEventAppended.Should().BeTrue();
    }

    public Task InitializeAsync() =>
        eventStore.Init();

    public async Task DisposeAsync() =>
        await eventStore.DisposeAsync();
}
