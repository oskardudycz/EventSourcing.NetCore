using Dapper.Contrib.Extensions;
using FluentAssertions;
using Newtonsoft.Json;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise09Projections
{
    public class User: Aggregate
    {
        public string Name { get; private set; } = default!;

        [JsonConstructor]
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

    public record UserCreated(
        Guid UserId,
        string UserName
    );

    public record UserNameUpdated(
        Guid UserId,
        string UserName
    );

    public class Order: Aggregate
    {
        public string Number { get; private set; } = default!;

        public decimal Amount { get; private set; }

        public Order(Guid id, Guid userId, string number, decimal price)
        {
            var @event = new OrderCreated(id, userId, number, price);

            Enqueue(@event);
            Apply(@event);
        }

        public void Apply(OrderCreated @event)
        {
            Id = @event.OrderId;
            Number = @event.Number;
            Amount = @event.Amount;
        }
    }

    public record OrderCreated(
        Guid OrderId,
        Guid UserId,
        string Number,
        decimal Amount
    );

    public record UserDashboard(
        Guid Id,
        string UserName,
        int OrdersCount,
        decimal TotalAmount
    );

    public class UserDashboardProjection(NpgsqlConnection databaseConnection): Projection
    {
        private readonly NpgsqlConnection databaseConnection = databaseConnection;

        public void Apply(UserCreated @event) =>
            throw new NotImplementedException("TODO");

        public void Apply(UserNameUpdated @event) =>
            throw new NotImplementedException("TODO");

        public void Apply(OrderCreated @event) =>
            throw new NotImplementedException("TODO");
    }

    [Migration(2, "Create Users dashboard table")]
    public class CreateUsersDashboard: Migration
    {
        protected override void Up() =>
            throw new NotImplementedException("TODO Create User Dashboard table");

        protected override void Down() =>
            throw new NotImplementedException("TODO drop Dashboard table");
    }

    private readonly NpgsqlConnection databaseConnection;
    private readonly EventStore eventStore;
    private readonly IRepository<User> userRepository;
    private readonly IRepository<Order> orderRepository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise09Projections()
    {
        databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

        var databaseProvider =
            new PostgresqlDatabaseProvider(databaseConnection) { SchemaName = typeof(Exercise09Projections).Name };

        var migrationsAssembly = typeof(Exercise09Projections).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        eventStore.AddProjection(new UserDashboardProjection(databaseConnection));

        // Initialize Event Store
        eventStore.Init();

        userRepository = new Repository<User>(eventStore);
        orderRepository = new Repository<Order>(eventStore);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
    public void AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var user = new User(Guid.NewGuid(), "John Doe");

        userRepository.Add(user);

        var firstOrder = new Order(Guid.NewGuid(), user.Id, "ORD/2019/08/01", 100.13M);
        var secondOrder = new Order(Guid.NewGuid(), user.Id, "ORD/2019/08/01", 2.110M);

        orderRepository.Add(firstOrder);
        orderRepository.Add(secondOrder);

        user.ChangeName("Alan Smith");

        userRepository.Update(user);

        var userDashboard = databaseConnection.Get<UserDashboard>(user.Id);

        userDashboard.Should().NotBeNull();
        userDashboard.Id.Should().Be(user.Id);
        userDashboard.UserName.Should().Be(user.Name);
        userDashboard.OrdersCount.Should().Be(2);
        userDashboard.TotalAmount.Should().Be(firstOrder.Amount + secondOrder.Amount);
    }
}
