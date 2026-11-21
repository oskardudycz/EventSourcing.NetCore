using OptimisticConcurrency.Core;
using Shouldly;
using Xunit;

namespace OptimisticConcurrency.RetryInInfrastructure.Core;

public class CommandHandlerTests
{
    [Fact]
    public async Task NothingStored_FunctionReceivesInitialState_AndResultIsStoredWithVersion1()
    {
        var received = -1;

        var events = await handler.Handle(id, state =>
        {
            received = state.Value;
            return [new Incremented(1)];
        }, Ct);

        received.ShouldBe(0);
        events.ShouldBe([new Incremented(1)]);
        var stored = (await database.Get<Counter>(id, Ct))!;
        stored.Document.ShouldBe(new Counter(1));
        stored.Version.ShouldBe(1);
    }

    [Fact]
    public async Task StoredState_FunctionReceivesIt_AndEvolvedStateIsStored()
    {
        await handler.Handle(id, _ => [new Incremented(1)], Ct);

        var events = await handler.Handle(id, state => [new Incremented(state.Value + 1)], Ct);

        events.ShouldBe([new Incremented(2)]);
        var stored = (await database.Get<Counter>(id, Ct))!;
        stored.Document.ShouldBe(new Counter(2));
        stored.Version.ShouldBe(2);
    }

    [Fact]
    public async Task FunctionReturningNoEvents_StoresNothing()
    {
        var events = await handler.Handle(id, _ => [], Ct);

        events.ShouldBeEmpty();
        (await database.Get<Counter>(id, Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task FunctionReturningNoEvents_KeepsStoredVersion()
    {
        await handler.Handle(id, _ => [new Incremented(1)], Ct);

        var events = await handler.Handle(id, _ => [], Ct);

        events.ShouldBeEmpty();
        (await database.Get<Counter>(id, Ct))!.Version.ShouldBe(1);
    }

    [Fact]
    public async Task Conflict_RunsFunctionAgainOnFreshlyLoadedStateAndReturnsOnlyLastAttemptEvents()
    {
        await handler.Handle(id, _ => [new Incremented(1)], Ct);
        var attempts = 0;

        var events = await handler.Handle(id, state =>
        {
            if (++attempts == 1)
                ConcurrentlyStoreCounterWithValue2();
            return [new Incremented(state.Value + 1)];
        }, Ct);

        attempts.ShouldBe(2);
        events.ShouldBe([new Incremented(3)]);
        (await database.Get<Counter>(id, Ct))!.Document.ShouldBe(new Counter(3));
    }

    [Fact]
    public async Task ConflictOnEveryAttempt_ThrowsAfterTenAttempts()
    {
        await handler.Handle(id, _ => [new Incremented(1)], Ct);
        var attempts = 0;

        await Should.ThrowAsync<ConcurrencyException>(async () =>
            await handler.Handle(id, state =>
            {
                attempts++;
                ConcurrentlyStoreCounterWithValue2();
                return [new Incremented(state.Value + 1)];
            }, Ct));

        attempts.ShouldBe(10);
    }

    private void ConcurrentlyStoreCounterWithValue2() =>
        database.Store(id, new Counter(2), Ct).AsTask().GetAwaiter().GetResult();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly Guid id = Guid.NewGuid();
    private readonly Database database = new();
    private readonly CommandHandler<Counter, Incremented> handler;

    public CommandHandlerTests() =>
        handler = new CommandHandler<Counter, Incremented>(database, new Counter(0), (_, @event) => new Counter(@event.Value));

    public record Incremented(int Value);

    public record Counter(int Value);
}
