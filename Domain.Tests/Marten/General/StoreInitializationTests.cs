using System.Linq;
using Marten;
using Marten.Events;
using SharpTestsEx;
using Xunit;

namespace Domain.Tests.Marten.General
{
    public class StoreInitializationTests
    {
        [Fact]
        public void GivenWrongConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
        {
            var ex = Record.Exception(() =>
            {
                var store = DocumentStore.For("WrongConnectionString");
            });

            ex.Should().Not.Be.Null();
        }

        [Fact]
        public void GivenProperConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
        {
            var ex = Record.Exception(() =>
            {
                var store = DocumentStore.For(Settings.ConnectionString);

                ConnectionShouldBeEstablished(store);
            });

            ex.Should().Be.Null();
        }

        [Fact]
        public void GivenProperConnectionString_WhenDocumentSessionIsInitializedWithDifferentShema_ThenConnectionIsCreated()
        {
            var ex = Record.Exception(() =>
            {
                var store = DocumentStore.For(_ =>
                {
                    _.Connection(Settings.ConnectionString);
                    _.AutoCreateSchemaObjects = AutoCreate.All;
                    _.DatabaseSchemaName = "EventSourcing";
                });

                ConnectionShouldBeEstablished(store);
            });

            ex.Should().Be.Null();
        }

        private static void ConnectionShouldBeEstablished(IDocumentStore store)
        {
            using (var session = store.OpenSession())
            {
                session.DocumentStore.Schema.EnsureStorageExists(typeof(EventStream));

                var events = session.Events.QueryAllRawEvents().ToList();

                events.Should().Not.Be.Null();
            }
        }
    }
}
