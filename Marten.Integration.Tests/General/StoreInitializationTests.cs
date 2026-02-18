using FluentAssertions;
using JasperFx;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.General;

public class StoreInitializationTests(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer, false, false)
{
    [Fact]
    public async Task GivenWrongConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
    {
        var ex = await Record.ExceptionAsync(() =>
        {
            var store = DocumentStore.For("WrongConnectionString");

            return ConnectionShouldBeEstablished(store);
        });

        ex.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenProperConnectionString_WhenDocumentSessionIsInitialized_ThenConnectionIsCreated()
    {
        var ex = await Record.ExceptionAsync(() =>
        {
            var store = DocumentStore.For(ConnectionString);

            return ConnectionShouldBeEstablished(store);
        });

        ex.Should().BeNull();
    }

    [Fact]
    public async Task GivenProperConnectionString_WhenDocumentSessionIsInitializedWithDifferentShema_ThenConnectionIsCreated()
    {
        var ex = await Record.ExceptionAsync(() =>
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = SchemaName;
            });

            return ConnectionShouldBeEstablished(store);
        });

        ex.Should().BeNull();
    }

    [Fact(Skip = "To investigate in Npgsql")]
    public async Task GivenProperConnectionString_WhenDocumentSessionIsCreatedAndTransactionIsCreated_ThenConnectionIsCreatedAndItsPossibleToMakeRollback()
    {
        var ex = await Record.ExceptionAsync(async () =>
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.Events.DatabaseSchemaName = SchemaName;
            });

            await using var session = store.LightweightSession();
            await using var transaction = await session.Connection.BeginTransactionAsync();
            await ConnectionShouldBeEstablished(store);

            await transaction.RollbackAsync();
        });

        ex.Should().BeNull();
    }

    private static async Task ConnectionShouldBeEstablished(IDocumentStore store)
    {
        await using var session = store.LightweightSession();
        var result = await session.QueryAsync<int>("SELECT 1");

        result.Should().NotBeNull();
    }
}
