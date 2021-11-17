using System;
using System.Linq;
using System.Linq.Expressions;
using Dapper;
using Dapper.Contrib.Extensions;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise08Snapshots
{
    public class User: Aggregate
    {
        public string Name { get; private set; } = default!;

        // added only for dapper deserialization needs
        private User() { }

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

    [Migration(1, "Create Users table")]
    public class CreateUsers: Migration
    {
        protected override void Up()
        {
            throw new NotImplementedException("TODO Create Users table");
        }

        protected override void Down()
        {
            throw new NotImplementedException("TODO drop users table");
        }
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
            new PostgresqlDatabaseProvider(databaseConnection) { SchemaName = typeof(Exercise08Snapshots).Name };

        var migrationsAssembly = typeof(Exercise08Snapshots).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        var userSnapshot = new SnapshotToTable<User>(
            databaseConnection,
            @"TODO write upsert here");

        eventStore.AddSnapshot(userSnapshot);

        // Initialize Event Store
        eventStore.Init();

        repository = new Repository<User>(eventStore);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
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
    [Trait("Category", "SkipCI")]
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
                    WHERE name LIKE '%" + john + "%'");

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