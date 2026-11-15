using Shouldly;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise04EventStoreMethods: IAsyncLifetime
{
    public record User(
        string Name
    );

    public record UserCreated(
        Guid UserId,
        string UserName
    );

    public record UserNameUpdated(
        Guid UserId,
        string UserName
    );

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise04EventStoreMethods()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);
    }

    [Fact]
    public async Task GetStreamState_ShouldReturnProperStreamInfo()
    {
        var streamId = Guid.CreateVersion7();
        var @event = new UserCreated(streamId,"John Doe");

        await eventStore.AppendEvent<User>(streamId, @event, ct: TestContext.Current.CancellationToken);

        var streamState = await eventStore.GetStreamState(streamId, TestContext.Current.CancellationToken);

        streamState.ShouldNotBeNull();
        streamState!.Id.ShouldBe(streamId);
        streamState.Type.ShouldBe(typeof(User));
        streamState.Version.ShouldBe(1);
    }

    [Fact]
    public async Task GetEvents_ShouldReturnAppendedEvents()
    {
        var streamId = Guid.CreateVersion7();
        var userCreated = new UserCreated(streamId, "John Doe");
        var userNameUpdated = new UserNameUpdated(streamId, "Adam Smith");

        await eventStore.AppendEvent<User>(streamId, userCreated, ct: TestContext.Current.CancellationToken);
        await eventStore.AppendEvent<User>(streamId, userNameUpdated, ct: TestContext.Current.CancellationToken);

        var events = await eventStore.GetEvents(streamId, ct: TestContext.Current.CancellationToken);

        events.Cast<object>().Count().ShouldBe(2);

        events.OfType<UserCreated>().ShouldContain(
            e => e.UserId == userCreated.UserId && e.UserName == userCreated.UserName);

        events.OfType<UserNameUpdated>().ShouldContain(
            e => e.UserId == userNameUpdated.UserId && e.UserName == userNameUpdated.UserName);
    }

    public ValueTask InitializeAsync() =>
        new(eventStore.Init());

    public async ValueTask DisposeAsync() =>
        await eventStore.DisposeAsync();
}
