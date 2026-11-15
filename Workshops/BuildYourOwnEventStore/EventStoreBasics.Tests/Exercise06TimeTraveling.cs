using Shouldly;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise06TimeTravelling: IAsyncLifetime
{
    internal class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = null!;
        public long Version { get; private set; }

        public User(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        // For deserialization
        private User() { }

        public void Apply(UserCreated @event)
        {
            Id = @event.UserId;
            Name = @event.UserName;
        }

        public void Apply(UserNameUpdated @event)
        {
            Name = @event.UserName;
        }
    }

    internal class UserCreated(Guid userId, string userName)
    {
        public Guid UserId { get; } = userId;
        public string UserName { get; } = userName;
    }


    internal class UserNameUpdated(Guid userId, string userName)
    {
        public Guid UserId { get; } = userId;
        public string UserName { get; } = userName;
    }

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise06TimeTravelling()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);
    }

    [Fact]
    public async Task AggregateStream_ShouldReturnSpecifiedVersionOfTheStream()
    {
        var streamId = Guid.CreateVersion7();
        var userCreated = new UserCreated(streamId, "John Doe");
        var userNameUpdated = new UserNameUpdated(streamId, "Adam Smith");
        var userNameUpdatedAgain = new UserNameUpdated(streamId, "Michael Newman");

        await eventStore.AppendEvent<User>(streamId, userCreated, ct: TestContext.Current.CancellationToken);
        await eventStore.AppendEvent<User>(streamId, userNameUpdated, ct: TestContext.Current.CancellationToken);
        await eventStore.AppendEvent<User>(streamId, userNameUpdatedAgain, ct: TestContext.Current.CancellationToken);

        var aggregateAtVersion1 = await eventStore.AggregateStream<User>(streamId, 1, ct: TestContext.Current.CancellationToken);

        aggregateAtVersion1!.Id.ShouldBe(streamId);
        aggregateAtVersion1.Name.ShouldBe(userCreated.UserName);
        aggregateAtVersion1.Version.ShouldBe(1);


        var aggregateAtVersion2 = await eventStore.AggregateStream<User>(streamId, 2, ct: TestContext.Current.CancellationToken);

        aggregateAtVersion2!.Id.ShouldBe(streamId);
        aggregateAtVersion2.Name.ShouldBe(userNameUpdated.UserName);
        aggregateAtVersion2.Version.ShouldBe(2);


        var aggregateAtVersion3 = await eventStore.AggregateStream<User>(streamId, 3, ct: TestContext.Current.CancellationToken);

        aggregateAtVersion3!.Id.ShouldBe(streamId);
        aggregateAtVersion3.Name.ShouldBe(userNameUpdatedAgain.UserName);
        aggregateAtVersion3.Version.ShouldBe(3);
    }

    public ValueTask InitializeAsync() =>
        new(eventStore.Init());

    public async ValueTask DisposeAsync() =>
        await eventStore.DisposeAsync();
}
