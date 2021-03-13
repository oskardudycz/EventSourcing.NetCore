using System;
using Dapper.Contrib.Extensions;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using SimpleMigrations;
using SimpleMigrations.DatabaseProvider;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class Exercise09Projections
    {
        public class User: Aggregate
        {
            public string Name { get; private set; }

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

        public class Order: Aggregate
        {
            public string Number { get; private set; }

            public decimal Amount { get; private set; }

            // added only for dapper deserialization needs
            private Order() { }

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

        public class OrderCreated
        {
            public Guid OrderId { get; }
            public Guid UserId { get; }
            public string Number { get; }
            public decimal Amount { get; }

            public OrderCreated(Guid orderId, Guid userId, string number, decimal amount)
            {
                OrderId = orderId;
                UserId = userId;
                Number = number;
                Amount = amount;
            }
        }

        public class UserDashboard
        {
            public Guid Id { get; }
            public string UserName { get; }
            public int OrdersCount { get; }
            public decimal TotalAmount { get; }

            public UserDashboard(Guid id, string userName, int ordersCount, decimal totalAmount)
            {
                Id = id;
                UserName = userName;
                OrdersCount = ordersCount;
                TotalAmount = totalAmount;
            }
        }

        public class UserDashboardProjection: Projection
        {
            private readonly NpgsqlConnection databaseConnection;

            public UserDashboardProjection(NpgsqlConnection databaseConnection)
            {
                this.databaseConnection = databaseConnection;
            }

            public void Apply(UserCreated @event)
            {
                throw new NotImplementedException("TODO");
            }

            public void Apply(UserNameUpdated @event)
            {
                throw new NotImplementedException("TODO");
            }

            public void Apply(OrderCreated @event)
            {
                throw new NotImplementedException("TODO");
            }
        }

        [Migration(2, "Create Users dashboard table")]
        public class CreateUsersDashboard: Migration
        {
            protected override void Up()
            {
                throw new NotImplementedException("TODO Create User Dashboard table");
            }

            protected override void Down()
            {
                throw new NotImplementedException("TODO drop Dashboard table");
            }
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
        [Trait("Category", "Exercise")]
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
}
