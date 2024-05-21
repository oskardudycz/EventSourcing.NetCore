using Core.EventStoreDB.Subscriptions.Checkpoints;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using Core.Testing.Fixtures;
using Xunit;

namespace Core.EventStoreDB.Tests.Subscriptions.Checkpoints;

using static ISubscriptionCheckpointRepository;

public class PostgresSubscriptionCheckpointRepositoryTests(PostgresContainerFixture fixture)
    : IClassFixture<PostgresContainerFixture>
{
    private readonly string subscriptionId = Guid.NewGuid().ToString("N");

    private readonly PostgresConnectionProvider connectionFactory =
        PostgresConnectionProvider.From(fixture.DataSource);

    [Fact]
    public async Task Store_InitialInsert_Success()
    {
        var checkpointTableCreator = new PostgresSubscriptionCheckpointSetup(fixture.DataSource);
        await checkpointTableCreator.EnsureCheckpointsTableExist(CancellationToken.None);
        var repository = new PostgresSubscriptionCheckpointRepository(connectionFactory);

        var result = await repository.Store(subscriptionId, 1, Checkpoint.None, CancellationToken.None);

        Assert.IsType<StoreResult.Success>(result);
    }

    [Fact]
    public async Task Store_UpdatePosition_Success()
    {
        var checkpointTableCreator = new PostgresSubscriptionCheckpointSetup(fixture.DataSource);
        await checkpointTableCreator.EnsureCheckpointsTableExist(CancellationToken.None);
        var repository = new PostgresSubscriptionCheckpointRepository(connectionFactory);

        await repository.Store(subscriptionId, 1, Checkpoint.None, CancellationToken.None);
        var result = await repository.Store(subscriptionId, 2, Checkpoint.From(1), CancellationToken.None);

        Assert.IsType<StoreResult.Success>(result);
    }

    [Fact]
    public async Task Store_IdempotentCheck_ReturnsZero()
    {
        var checkpointTableCreator = new PostgresSubscriptionCheckpointSetup(fixture.DataSource);
        await checkpointTableCreator.EnsureCheckpointsTableExist(CancellationToken.None);
        var repository = new PostgresSubscriptionCheckpointRepository(connectionFactory);

        await repository.Store(subscriptionId, 1, Checkpoint.None, CancellationToken.None);
        await repository.Store(subscriptionId, 2, Checkpoint.From(1), CancellationToken.None);
        var result = await repository.Store(subscriptionId, 2, Checkpoint.From(1), CancellationToken.None);

        Assert.IsType<StoreResult.Ignored>(result);
    }

    [Fact]
    public async Task Store_InvalidUpdate_Failure()
    {
        var checkpointTableCreator = new PostgresSubscriptionCheckpointSetup(fixture.DataSource);
        await checkpointTableCreator.EnsureCheckpointsTableExist(CancellationToken.None);
        var repository = new PostgresSubscriptionCheckpointRepository(connectionFactory);

        await repository.Store(subscriptionId, 1, Checkpoint.None, CancellationToken.None);
        await repository.Store(subscriptionId, 2, Checkpoint.From(1), CancellationToken.None);
        var result = await repository.Store(subscriptionId, 1, Checkpoint.From(3), CancellationToken.None);

        Assert.IsType<StoreResult.Mismatch>(result);
    }
}
