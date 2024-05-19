using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise08Snapshots
{
    class User : Aggregate
    {
        public string Name { get; private set; } = default!;

        // added only for dapper deserialization needs
        private User() {}

        public User(Guid id, string name)
        {
            var @event = new UserCreated(id, name);

            Enqueue(@event);
            Apply(@event);
        }

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

    [Migration(1, "Create Users table")]
    public class CreateUsers : Migration
    {
        protected override void Up() =>
            Execute(@"CREATE TABLE users (
                      id             UUID                      NOT NULL    PRIMARY KEY,
                      name           TEXT                      NOT NULL,
                      version        BIGINT                    NOT NULL
                  );");

        protected override void Down() =>
            Execute("DROP TABLE users");
    }

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<User> repository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise08Snapshots()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        var databaseProvider =
            new PostgresqlDatabaseProvider(databaseConnection) {SchemaName = typeof(Exercise08Snapshots).Name};

        var migrationsAssembly = typeof(Exercise08Snapshots).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        var userSnapshot = new SnapshotToTable<User>(
            databaseConnection,
            @"INSERT INTO users (id, name, version) VALUES (@Id, @Name, @Version)
                 ON CONFLICT (id)
                 DO UPDATE SET name = @Name, version = @Version");

        eventStore.AddSnapshot(userSnapshot);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<User>(eventStore);
    }

    [Fact]
    public void AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var streamId = Guid.NewGuid();
        var user = new User(streamId, "John Doe");

        repository.Add(user);

        var userFromDb = databaseConnection.Get<User>(streamId);

        userFromDb.Should().NotBeNull();
        userFromDb.Id.Should().Be(streamId);
        userFromDb.Name.Should().Be("John Doe");
        userFromDb.Version.Should().Be(1);

        userFromDb.ChangeName("Adam Smith");

        repository.Update(userFromDb);

        var userAfterUpdate = databaseConnection.Get<User>(streamId);

        userAfterUpdate.Id.Should().Be(streamId);
        userAfterUpdate.Name.Should().Be("Adam Smith");
        userFromDb.Version.Should().Be(2);
    }



    [Fact]
    public void Snapshots_ShouldBeQueryable()
    {
        const string john = "John";

        var firstMatchingUser = new User(Guid.NewGuid(), $"{john} Doe");
        var secondMatchingUser = new User(Guid.NewGuid(), $"{john} Smith");
        var notMatchingUser = new User(Guid.NewGuid(), "Anna Smith");

        repository.Add(firstMatchingUser);
        repository.Add(secondMatchingUser);
        repository.Add(notMatchingUser);


        var users = databaseConnection.Query<User>(
            @"SELECT id, name, version
                    FROM USERS
                    WHERE name LIKE '%" + john + "%'" );

        users.Count().Should().Be(2);

        Expression<Func<User, bool>> UserEqualTo(User userToCompare)
        {
            return x => x.Id == userToCompare.Id
                        && x.Name == userToCompare.Name
                        && x.Version == userToCompare.Version;
        }

        users.Should().Contain(UserEqualTo(firstMatchingUser));
        users.Should().Contain(UserEqualTo(secondMatchingUser));
        users.Should().NotContain(UserEqualTo(notMatchingUser));
    }
}
