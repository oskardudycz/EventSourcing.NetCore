using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise06TimeTravelling
{
    public class User
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

    public class UserCreated
    {
        public Guid UserId { get; }
        public string UserName { get; }

        public UserCreated(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
    }

    public class UserNameUpdated
    {
        public Guid UserId { get; }
        public string UserName { get; }

        public UserNameUpdated(Guid userId, string userName)
        {
            UserId = userId;
            UserName = userName;
        }
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

        // Initialize Event Store
        eventStore.Init();
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AggregateStream_ShouldReturnSpecifiedVersionOfTheStream()
    {
        var streamId = Guid.NewGuid();
        var userCreated = new UserCreated(streamId, "John Doe");
        var userNameUpdated = new UserNameUpdated(streamId, "Adam Smith");
        var userNameUpdatedAgain = new UserNameUpdated(streamId, "Michael Newman");

        eventStore.AppendEvent<User>(streamId, userCreated);
        eventStore.AppendEvent<User>(streamId, userNameUpdated);
        eventStore.AppendEvent<User>(streamId, userNameUpdatedAgain);

        var aggregateAtVersion1 = eventStore.AggregateStream<User>(streamId, 1);

        aggregateAtVersion1.Id.Should().Be(streamId);
        aggregateAtVersion1.Name.Should().Be(userCreated.UserName);
        aggregateAtVersion1.Version.Should().Be(1);

        var aggregateAtVersion2 = eventStore.AggregateStream<User>(streamId, 2);

        aggregateAtVersion2.Id.Should().Be(streamId);
        aggregateAtVersion2.Name.Should().Be(userNameUpdated.UserName);
        aggregateAtVersion2.Version.Should().Be(2);

        var aggregateAtVersion3 = eventStore.AggregateStream<User>(streamId, 3);

        aggregateAtVersion3.Id.Should().Be(streamId);
        aggregateAtVersion3.Name.Should().Be(userNameUpdatedAgain.UserName);
        aggregateAtVersion3.Version.Should().Be(3);
    }
}
