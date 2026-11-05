using Core.Testing.Fixtures;
using Npgsql;
using Xunit;

namespace Core.EntityFramework.Tests.Fixtures;

public class PostgresContainerFixtureTests
{
    [Fact]
    public async Task DisposingFixture_KeepsSharedContainerAvailableForOtherFixtures()
    {
        var first = new PostgresContainerFixture();
        var second = new PostgresContainerFixture();

        await first.InitializeAsync();
        try
        {
            await second.InitializeAsync();
            try
            {
                Assert.Equal(first.DataSource.ConnectionString, second.DataSource.ConnectionString);

                await first.DisposeAsync();

                await using var connection = await second.DataSource.OpenConnectionAsync();
                await using var command = new NpgsqlCommand("select 1", connection);
                Assert.Equal(1, await command.ExecuteScalarAsync());
            }
            finally
            {
                await second.DisposeAsync();
            }
        }
        finally
        {
            await first.DisposeAsync();
        }
    }
}
