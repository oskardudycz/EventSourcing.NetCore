using FluentAssertions;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise06TimeTravelling: IAsyncLifetime
{
    class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; } = default!;
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

    class UserCreated(Guid userId, string userName)
    {
        public Guid UserId { get; } = userId;
        public string UserName { get; } = userName;
    }


    class UserNameUpdated(Guid userId, string userName)
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
        var streamId = Guid.NewGuid();
        var userCreated = new UserCreated(streamId, "John Doe");
        var userNameUpdated = new UserNameUpdated(streamId, "Adam Smith");
        var userNameUpdatedAgain = new UserNameUpdated(streamId, "Michael Newman");

        await eventStore.AppendEvent<User>(streamId, userCreated);
        await eventStore.AppendEvent<User>(streamId, userNameUpdated);
        await eventStore.AppendEvent<User>(streamId, userNameUpdatedAgain);

        var aggregateAtVersion1 = await eventStore.AggregateStream<User>(streamId, 1);

        aggregateAtVersion1!.Id.Should().Be(streamId);
        aggregateAtVersion1.Name.Should().Be(userCreated.UserName);
        aggregateAtVersion1.Version.Should().Be(1);


        var aggregateAtVersion2 = await eventStore.AggregateStream<User>(streamId, 2);

        aggregateAtVersion2!.Id.Should().Be(streamId);
        aggregateAtVersion2.Name.Should().Be(userNameUpdated.UserName);
        aggregateAtVersion2.Version.Should().Be(2);


        var aggregateAtVersion3 = await eventStore.AggregateStream<User>(streamId, 3);

        aggregateAtVersion3!.Id.Should().Be(streamId);
        aggregateAtVersion3.Name.Should().Be(userNameUpdatedAgain.UserName);
        aggregateAtVersion3.Version.Should().Be(3);
    }

    public Task InitializeAsync() =>
        eventStore.Init();

    public async Task DisposeAsync() =>
        await eventStore.DisposeAsync();
}
