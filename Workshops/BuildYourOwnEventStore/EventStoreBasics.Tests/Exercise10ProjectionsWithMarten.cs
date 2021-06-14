using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Marten;
using Marten.Events.Projections;
using Weasel.Postgresql;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class User: Aggregate
    {
        public string Name { get; private set; } = default!;

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
        public string Number { get; private set; } = default!;

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
        public Guid Id { get; set; }
        public string UserName { get; set; } = default!;
        public int OrdersCount { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class UserDashboardProjection: ViewProjection<UserDashboard, Guid>
    {
        public UserDashboardProjection()
        {
            Identity<UserCreated>(e => e.UserId);

            //Identity<UserNameUpdated>(e => e.UserId);

            //Identity<OrderCreated>(e => e.UserId);
        }

        // public UserDashboard Create(UserCreated @event)
        // {
        //     var item = new UserDashboard();
        //     item.Id = @event.UserId;
        //     item.UserName = @event.UserName;
        //     item.OrdersCount = 0;
        //     item.TotalAmount = 0;
        //     return item;
        // }

        public void Apply(UserDashboard item, UserCreated @event)
        {
            item.Id = @event.UserId;
            item.UserName = @event.UserName;
            item.OrdersCount = 0;
            item.TotalAmount = 0;
        }

        public void Apply(UserNameUpdated @event, UserDashboard item)
        {
            item.UserName = @event.UserName;
        }

        // public void Apply(OrderCreated @event, UserDashboard item)
        // {
        //     item.TotalAmount += @event.Amount;
        //     item.OrdersCount++;
        // }
    }

    public class Exercise10ProjectionsWithMarten
    {
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
                options.DatabaseSchemaName =
                    options.Events.DatabaseSchemaName = nameof(Exercise10ProjectionsWithMarten);
                options.Projections.SelfAggregate<User>();
                options.Projections.SelfAggregate<Order>();
                options.Projections.Add<UserDashboardProjection>();

                // options.Events.AddEventTypes(new[] {typeof(UserCreated)});
                // options.Projections.Add(new UserDashboardProjection());
            });

            documentSession = store.OpenSession();

            userRepository = new MartenRepository<User>(documentSession);
            orderRepository = new MartenRepository<Order>(documentSession);
        }

        [Fact(Skip = "for now")]
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

            var userDashboard = documentSession.Load<UserDashboard>(user.Id);

            userDashboard.Should().NotBeNull();
            userDashboard!.Id.Should().Be(user.Id);
            userDashboard.UserName.Should().Be(user.Name);
            userDashboard.OrdersCount.Should().Be(2);
            userDashboard.TotalAmount.Should().Be(firstOrder.Amount + secondOrder.Amount);
        }
    }
}
