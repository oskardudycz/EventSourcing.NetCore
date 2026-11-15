using Shouldly;
using Npgsql;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise07AggregateAndRepository: IAsyncLifetime
{

    internal class User : Aggregate
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

        await repository.Add(user, TestContext.Current.CancellationToken);

        var userFromRepository = await repository.Find(streamId, TestContext.Current.CancellationToken);

        userFromRepository.ShouldNotBeNull();
        userFromRepository!.Id.ShouldBe(streamId);
        userFromRepository.Name.ShouldBe("John Doe");
        userFromRepository.Version.ShouldBe(1);

        userFromRepository.ChangeName("Adam Smith");

        await repository.Update(userFromRepository, TestContext.Current.CancellationToken);

        var userAfterUpdate = await repository.Find(streamId, TestContext.Current.CancellationToken);

        userAfterUpdate.ShouldNotBeNull();
        userAfterUpdate!.Id.ShouldBe(streamId);
        userAfterUpdate.Name.ShouldBe("Adam Smith");
        userFromRepository.Version.ShouldBe(2);
    }

    public ValueTask InitializeAsync() =>
        new(eventStore.Init());

    public async ValueTask DisposeAsync() =>
        await eventStore.DisposeAsync();
}
