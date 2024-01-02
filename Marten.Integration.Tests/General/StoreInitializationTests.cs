using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Npgsql;
using Weasel.Core;
using Xunit;

namespace Marten.Integration.Tests.General;

public class StoreInitializationTests: MartenTest
{
    [Fact]
    public void GivenWrongConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
    {
        var ex = Record.Exception(() =>
        {
            var store = DocumentStore.For("WrongConnectionString");

            ConnectionShouldBeEstablished(store);
        });

        ex.Should().NotBeNull();
    }

    [Fact]
    public void GivenProperConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
    {
        var ex = Record.Exception(() =>
        {
            var store = DocumentStore.For(ConnectionString);

            ConnectionShouldBeEstablished(store);
        });

        ex.Should().BeNull();
    }

    [Fact]
    public void GivenProperConnectionString_WhenDocumentSessionIsInitializedWithDifferentShema_ThenConnectionIsCreated()
    {
        var ex = Record.Exception(() =>
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = SchemaName;
            });

            ConnectionShouldBeEstablished(store);
        });

        ex.Should().BeNull();
    }

    [Fact(Skip = "To investigate in Npgsql")]
    public void GivenProperConnectionString_WhenDocumentSessionIsCreatedAndTransactionIsCreated_ThenConnectionIsCreatedAndItsPossibleToMakeRollback()
    {
        var ex = Record.Exception(() =>
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = SchemaName;
            });

            using var session = store.LightweightSession();
            using var transaction = session.Connection!.BeginTransaction();
            ConnectionShouldBeEstablished(store);

            transaction.Rollback();
        });

        ex.Should().BeNull();
    }

    private static void ConnectionShouldBeEstablished(IDocumentStore store)
    {
        using var session = store.LightweightSession();
        var result = session.Query<int>("SELECT 1");

        result.Should().NotBeNull();
    }

    public StoreInitializationTests(MartenFixture fixture) : base(fixture.PostgreSqlContainer, false, false)
    {

    }
}
