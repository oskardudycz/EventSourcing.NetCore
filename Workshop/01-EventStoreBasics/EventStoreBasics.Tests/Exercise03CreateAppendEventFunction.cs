using System;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class User
    {
        public string Name { get; set; }
    }


    public class UserCreated
    {
        public string Name { get; set; }
    }

    public class Exercise03CreateAppendEventFunction
    {
        private readonly NpgsqlConnection databaseConnection;
        private readonly PostgresSchemaProvider schemaProvider;
        private readonly EventStore eventStore;

        /// <summary>
        /// Inits Event Store
        /// </summary>
        public Exercise03CreateAppendEventFunction()
        {
            databaseConnection = PostgresDbConnectionProvider.GetFreshDbConnection();
            schemaProvider = new PostgresSchemaProvider(databaseConnection);

            // Create Event Store
            eventStore = new EventStore(databaseConnection);

            // Initialize Event Store
            eventStore.Init();
        }

        [Fact]
        public void AppendEventFunction_ShouldBeCreated()
        {
            var streamId = Guid.NewGuid();
            var @event = new UserCreated {Name = "John Doe"};

            var result = eventStore.AppendEvent<User, UserCreated>(streamId, @event);

            result.Should().BeTrue();
        }
    }
}
