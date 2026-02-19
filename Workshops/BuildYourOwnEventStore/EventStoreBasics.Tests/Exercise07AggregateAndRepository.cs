using FluentAssertions;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise07AggregateAndRepository: IAsyncLifetime
{

    class User : Aggregate
    {
        public string Name { get; private set; } = null!;

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
    private readonly IRepository<User> repository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise07AggregateAndRepository()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        repository = new Repository<User>(eventStore);
    }

    [Fact]
    public async Task Repository_FullFlow_ShouldSucceed()
    {
        var streamId = Guid.CreateVersion7();
        var user = new User(streamId, "John Doe");

        await repository.Add(user);

        var userFromRepository = await repository.Find(streamId);

        userFromRepository.Should().NotBeNull();
        userFromRepository!.Id.Should().Be(streamId);
        userFromRepository.Name.Should().Be("John Doe");
        userFromRepository.Version.Should().Be(1);

        userFromRepository.ChangeName("Adam Smith");

        await repository.Update(userFromRepository);

        var userAfterUpdate = await repository.Find(streamId);

        userAfterUpdate.Should().NotBeNull();
        userAfterUpdate!.Id.Should().Be(streamId);
        userAfterUpdate.Name.Should().Be("Adam Smith");
        userFromRepository.Version.Should().Be(2);
    }

    public Task InitializeAsync() =>
        eventStore.Init();

    public async Task DisposeAsync() =>
        await eventStore.DisposeAsync();
}
