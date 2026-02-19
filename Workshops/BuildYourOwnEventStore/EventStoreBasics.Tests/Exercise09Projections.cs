using Dapper;
using Dapper.Contrib.Extensions;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Tools.Tools;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise09Projections: IAsyncLifetime
{
    class User : Aggregate
    {
        public string Name { get; private set; } = null!;

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

    class Order : Aggregate
    {
        public string Number { get; private set; } = null!;

        public decimal Amount { get; private set; }

        // added only for dapper deserialization needs
        private Order() {}

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

    public class OrderCreated(Guid orderId, Guid userId, string number, decimal amount)
    {
        public Guid OrderId { get; } = orderId;
        public Guid UserId { get; } = userId;
        public string Number { get; } = number;
        public decimal Amount { get; } = amount;
    }

    public class UserDashboard(Guid id, string userName, int ordersCount, decimal totalAmount)
    {

        public Guid Id { get; } = id;
        public string UserName { get; } = userName;
        public int OrdersCount { get; } = ordersCount;
        public decimal TotalAmount { get; } = totalAmount;
    }

    public class UserDashboardProjection : Projection
    {
        private readonly NpgsqlConnection databaseConnection;

        public UserDashboardProjection(NpgsqlConnection databaseConnection)
        {
            this.databaseConnection = databaseConnection;

            Projects<UserCreated>(Apply);
            Projects<UserNameUpdated>(Apply);
            Projects<OrderCreated>(Apply);
        }

        void Apply(UserCreated @event) =>
            databaseConnection.Execute(
                @"INSERT INTO UserDashboards (Id, UserName, OrdersCount, TotalAmount)
                    VALUES (@UserId, @UserName, 0, 0)",
                @event
            );

        void Apply(UserNameUpdated @event) =>
            databaseConnection.Execute(
                @"UPDATE UserDashboards
                    SET UserName = @UserName
                    WHERE Id = @UserId",
                @event
            );

        void Apply(OrderCreated @event) =>
            databaseConnection.Execute(
                @"UPDATE UserDashboards
                    SET OrdersCount = OrdersCount + 1,
                        TotalAmount = TotalAmount + @Amount
                    WHERE Id = @UserId",
                @event
            );
    }

    [Migration(2, "Create Users dashboard table")]
    public class CreateUsersDashboard : Migration
    {
        protected override void Up() =>
            Execute(@"CREATE TABLE UserDashboards (
                      Id            UUID                      NOT NULL    PRIMARY KEY,
                      UserName      TEXT                      NOT NULL,
                      OrdersCount   integer                   NOT NULL,
                      TotalAmount   decimal                   NOT NULL
                  );");

        protected override void Down() =>
            Execute("DROP TABLE UserDashboards");
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
            new PostgresqlDatabaseProvider(databaseConnection) {SchemaName = typeof(Exercise09Projections).Name};

        var migrationsAssembly = typeof(Exercise09Projections).Assembly;
        var migrator = new SimpleMigrator(migrationsAssembly, databaseProvider);
        migrator.Load();
        migrator.MigrateToLatest();

        // Create Event Store
        eventStore = new EventStore(databaseConnection);

        eventStore.AddProjection(new UserDashboardProjection(databaseConnection));

        userRepository = new Repository<User>(eventStore);
        orderRepository = new Repository<Order>(eventStore);
    }

    [Fact]
    public async Task AddingAndUpdatingAggregate_ShouldCreateAndUpdateSnapshotAccordingly()
    {
        var user = new User(Guid.CreateVersion7(), "John Doe");

        await userRepository.Add(user);

        var firstOrder = new Order(Guid.CreateVersion7(), user.Id, "ORD/2019/08/01", 100.13M);
        var secondOrder = new Order(Guid.CreateVersion7(), user.Id, "ORD/2019/08/01", 2.110M);

        await orderRepository.Add(firstOrder);
        await orderRepository.Add(secondOrder);

        user.ChangeName("Alan Smith");

        await userRepository.Update(user);

        var userDashboard = await databaseConnection.GetAsync<UserDashboard>(user.Id);

        userDashboard.Should().NotBeNull();
        userDashboard.Id.Should().Be(user.Id);
        userDashboard.UserName.Should().Be(user.Name);
        userDashboard.OrdersCount.Should().Be(2);
        userDashboard.TotalAmount.Should().Be(firstOrder.Amount + secondOrder.Amount);
    }

    public Task InitializeAsync() =>
        eventStore.Init();

    public async Task DisposeAsync() =>
        await eventStore.DisposeAsync();
}
