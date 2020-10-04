using System;
using System.Linq;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class Exercise04EventStoreMethods
    {
        private class User
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
        }

        private class UserCreated
        {
            public Guid UserId { get; }
            public string UserName { get; }

            public UserCreated(Guid userId, string userName)
            {
                UserId = userId;
                UserName = userName;
            }
        }

        private class UserNameUpdated
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
        private readonly EventStore eventStore;

        /// <summary>
        /// Inits Event Store
        /// </summary>
        public Exercise04EventStoreMethods()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();

            // Create Event Store
            eventStore = new EventStore(databaseConnection);

            // Initialize Event Store
            eventStore.Init();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public void GetStreamState_ShouldReturnProperStreamInfo()
        {
            var streamId = Guid.NewGuid();
            var @event = new UserCreated(streamId, "John Doe");

            eventStore.AppendEvent<User>(streamId, @event);

            var streamState = eventStore.GetStreamState(streamId);

            streamState.Id.Should().Be(streamId);
            streamState.Type.Should().Be(typeof(User));
            streamState.Version.Should().Be(1);
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public void GetEvents_ShouldReturnAppendedEvents()
        {
            var streamId = Guid.NewGuid();
            var userCreated = new UserCreated(streamId, "John Doe");
            var userNameUpdated = new UserNameUpdated(streamId, "Adam Smith");

            eventStore.AppendEvent<User>(streamId, userCreated);
            eventStore.AppendEvent<User>(streamId, userNameUpdated);

            var events = eventStore.GetEvents(streamId);

            events.Should().HaveCount(2);

            events.OfType<UserCreated>().Should().Contain(
                e => e.UserId == userCreated.UserId && e.UserName == userCreated.UserName);

            events.OfType<UserNameUpdated>().Should().Contain(
                e => e.UserId == userNameUpdated.UserId && e.UserName == userNameUpdated.UserName);
        }
    }
}
