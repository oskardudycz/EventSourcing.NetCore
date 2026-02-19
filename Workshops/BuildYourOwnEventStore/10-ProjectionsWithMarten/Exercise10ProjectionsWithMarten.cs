using FluentAssertions;
using JasperFx;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Tools.Tools;
using Weasel.Core;
using Xunit;

namespace EventStoreBasics.Tests;

public class Exercise10ProjectionsWithMarten
{
    public class User: Aggregate
    {
        public string Name { get; private set; } = null!;

        // added only for dapper deserialization needs
        public User() { }

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

        public void Apply(UserNameUpdated @event) =>
            Name = @event.UserName;
    }

    public class UserCreated(Guid userId, string userName)
    {
        public Guid UserId { get; } = userId;
        public string UserName { get; } = userName;
    }

    public class UserNameUpdated(Guid userId, string userName)
    {
        public Guid UserId { get; } = userId;
        public string UserName { get; } = userName;
    }

    public class Order: Aggregate
    {
        public string Number { get; private set; } = null!;

        public decimal Amount { get; private set; }

        // added only for Marten deserialization needs
        public Order() { }

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

    public class UserDashboard
    {
        public Guid Id { get; set; }
        public string UserName { get; set; } = null!;
        public int OrdersCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UserDashboardProjection: MultiStreamProjection<UserDashboard, Guid>
    {
        public UserDashboardProjection()
        {
            Identity<UserCreated>(e => e.UserId);
            ProjectEvent<UserCreated>(Apply);

            Identity<UserNameUpdated>(e => e.UserId);
            ProjectEvent<UserNameUpdated>(Apply);

            Identity<OrderCreated>(e => e.UserId);
            ProjectEvent<OrderCreated>(Apply);
        }

        public void Apply(UserDashboard item, UserCreated @event)
        {
            item.Id = @event.UserId;
            item.UserName = @event.UserName;
            item.OrdersCount = 0;
            item.TotalAmount = 0;
        }

        public void Apply(UserDashboard item, UserNameUpdated @event)
        {
            item.UserName = @event.UserName;
        }

        public void Apply(UserDashboard item, OrderCreated @event)
        {
            item.TotalAmount += @event.Amount;
            item.OrdersCount++;
        }
    }

    private readonly IDocumentSession documentSession;
    private readonly IRepository<User> userRepository;
    private readonly IRepository<Order> orderRepository;

    /// <summary>
    /// Inits Event Store
    /// </summary>
    public Exercise10ProjectionsWithMarten()
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(Settings.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = options.Events.DatabaseSchemaName = nameof(Exercise10ProjectionsWithMarten);
            options.Projections.Snapshot<User>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<Order>(SnapshotLifecycle.Inline);
            options.Projections.Add<UserDashboardProjection>(ProjectionLifecycle.Inline);
        });

        documentSession = store.LightweightSession();

        userRepository = new MartenRepository<User>(documentSession);
        orderRepository = new MartenRepository<Order>(documentSession);
    }

    [Fact]
    [Trait("Category", "SkipCI")]
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

        var userDashboard = await documentSession.LoadAsync<UserDashboard>(user.Id);

        userDashboard.Should().NotBeNull();
        userDashboard!.Id.Should().Be(user.Id);
        userDashboard.UserName.Should().Be(user.Name);
        userDashboard.OrdersCount.Should().Be(2);
        userDashboard.TotalAmount.Should().Be(firstOrder.Amount + secondOrder.Amount);
    }
}
