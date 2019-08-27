using System;
using Dapper;
using EventStoreBasics.Tests.Tools;
using FluentAssertions;
using Npgsql;
using Xunit;

namespace EventStoreBasics.Tests
{
    public class Exercise03CreateAppendEventFunction
    {
        private class User
        {
            public string Name { get; set; }
        }

        private class UserCreated
        {
            public string Name { get; }

            public UserCreated(string name)
            {
                Name = name;
            }
        }

        private readonly NpgsqlConnection databaseConnection;
        private readonly PostgresSchemaProvider schemaProvider;
        private readonly EventStore eventStore;

        private const string AppendEventFunctionName = "append_event";

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
        [Trait("Category", "Exercise")]
        public void AppendEventFunction_ShouldBeCreated()
        {
            var appendFunctionExists = schemaProvider
                .FunctionExists(AppendEventFunctionName);

            appendFunctionExists.Should().BeTrue();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public void AppendEventFunction_WhenStreamDoesNotExist_CreateNewStream_And_AppendNewEvent()
        {
            var streamId = Guid.NewGuid();
            var @event = new UserCreated("John Doe");

            var result = eventStore.AppendEvent<User>(streamId, @event);

            result.Should().BeTrue();

            var wasStreamCreated = databaseConnection.QuerySingle<bool>(
                "select exists (select 1 from streams where id = @streamId)", new { streamId }
            );
            wasStreamCreated.Should().BeTrue();

            var wasEventAppended = databaseConnection.QuerySingle<bool>(
                "select exists (select 1 from events where stream_id = @streamId)", new { streamId }
            );
            wasEventAppended.Should().BeTrue();
        }
    }
}
