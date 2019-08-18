using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class Exercise06AggregateAndRepository
    {

        class User : Aggregate
        {
            public string Name { get; private set; }

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

            private void Apply(UserCreated @event)
            {
                Id = @event.UserId;
                Name = @event.UserName;
            }

            private void Apply(UserNameUpdated @event)
            {
                Name = @event.UserName;
            }
        }

        class UserCreated
        {
            public Guid UserId { get; }
            public string UserName { get; }

            public UserCreated(Guid userId, string userName)
            {
                UserId = userId;
                UserName = userName;
            }
        }


        class UserNameUpdated
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
        private readonly PostgresSchemaProvider schemaProvider;
        private readonly EventStore eventStore;
        private readonly IRepository<User> repository;

        /// <summary>
        /// Inits Event Store
        /// </summary>
        public Exercise06AggregateAndRepository()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
            schemaProvider = new PostgresSchemaProvider(databaseConnection);

            // Create Event Store
            eventStore = new EventStore(databaseConnection);

            // Initialize Event Store
            eventStore.Init();

            repository = new Repository<User>(eventStore);
        }

        [Fact]
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
}
