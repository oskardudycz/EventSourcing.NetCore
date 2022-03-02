using System;
using FluentAssertions;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise07AggregateAndRepository
{
    public class User: Aggregate
    {
        public string Name { get; private set; } = default!;

        public User(Guid id, string name)
        {
            var @event = new UserCreated(id, name);

            Enqueue(@event);
            Apply(@event);
        }

        // For serialization
        private User() { }

        public void ChangeName(string name)
        {
            var @event = new UserNameUpdated(Id, name);

            Enqueue(@event);
            Apply(@event);
        }

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
    private readonly IRepository<User> repository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise07AggregateAndRepository()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<User>(eventStore);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void Repository_FullFlow_ShouldSucceed()
    {
        var streamId = Guid.NewGuid();
        var user = new User(streamId, "John Doe");

        repository.Add(user);

        var userFromRepository = repository.Find(streamId);

        userFromRepository.Id.Should().Be(streamId);
        userFromRepository.Name.Should().Be("John Doe");
        userFromRepository.Version.Should().Be(1);

        userFromRepository.ChangeName("Adam Smith");

        repository.Update(userFromRepository);

        var userAfterUpdate = repository.Find(streamId);

        userAfterUpdate.Id.Should().Be(streamId);
        userAfterUpdate.Name.Should().Be("Adam Smith");
        userFromRepository.Version.Should().Be(2);
    }
}