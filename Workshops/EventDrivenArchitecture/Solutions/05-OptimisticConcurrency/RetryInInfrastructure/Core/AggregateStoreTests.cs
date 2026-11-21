using System.Text.Json.Serialization;
using OptimisticConcurrency.Core;
using Shouldly;
using Xunit;

namespace OptimisticConcurrency.RetryInInfrastructure.Core;

public class AggregateStoreTests
{
    [Fact]
    public async Task FunctionReturningNewAggregate_StoresItWithVersion1AndReturnsItsEvents()
    {
        var events = await store.Handle(id, _ => Counter.Create(id), Ct);

        events.ShouldBe([new Incremented(1)]);
        (await database.Get<Counter>(id, Ct))!.Version.ShouldBe(1);
    }

    [Fact]
    public async Task FunctionReceivesLoadedAggregate_AndItsChangesAreStored()
    {
        await store.Handle(id, _ => Counter.Create(id), Ct);

        var events = await store.Handle(id, counter => counter!.Increment(), Ct);

        events.ShouldBe([new Incremented(2)]);
        var stored = (await database.Get<Counter>(id, Ct))!;
        stored.Document.Value.ShouldBe(2);
        stored.Version.ShouldBe(2);
    }

    [Fact]
    public async Task NothingStored_FunctionReceivesNull()
    {
        var received = new List<Counter?>();

        await store.Handle(id, counter =>
        {
            received.Add(counter);
            return null;
        }, Ct);

        received.ShouldBe([null]);
    }

    [Fact]
    public async Task FunctionReturningNothing_StoresNothingAndReturnsNoEvents()
    {
        var events = await store.Handle(id, _ => null, Ct);

        events.ShouldBeEmpty();
        (await database.Get<Counter>(id, Ct)).ShouldBeNull();
    }

    [Fact]
    public async Task AggregateWithoutEvents_IsNotStored()
    {
        await store.Handle(id, _ => Counter.Create(id), Ct);

        var events = await store.Handle(id, counter => counter, Ct);

        events.ShouldBeEmpty();
        (await database.Get<Counter>(id, Ct))!.Version.ShouldBe(1);
    }

    [Fact]
    public async Task Conflict_RunsFunctionAgainOnFreshlyLoadedAggregateAndReturnsOnlyLastAttemptEvents()
    {
        await store.Handle(id, _ => Counter.Create(id), Ct);
        var attempts = 0;

        var events = await store.Handle(id, counter =>
        {
            if (++attempts == 1)
                ConcurrentlyStoreCounterWithValue2();
            return counter!.Increment();
        }, Ct);

        attempts.ShouldBe(2);
        events.ShouldBe([new Incremented(3)]);
        (await database.Get<Counter>(id, Ct))!.Document.Value.ShouldBe(3);
    }

    [Fact]
    public async Task ConflictOnEveryAttempt_ThrowsAfterTenAttempts()
    {
        await store.Handle(id, _ => Counter.Create(id), Ct);
        var attempts = 0;

        await Should.ThrowAsync<ConcurrencyException>(async () =>
            await store.Handle(id, counter =>
            {
                attempts++;
                ConcurrentlyStoreCounterWithValue2();
                return counter!.Increment();
            }, Ct));

        attempts.ShouldBe(10);
    }

    private void ConcurrentlyStoreCounterWithValue2() =>
        database.Store(id, Counter.Create(id).Increment(), Ct).AsTask().GetAwaiter().GetResult();

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private readonly Guid id = Guid.NewGuid();
    private readonly Database database = new();
    private readonly AggregateStore<Counter, Incremented> store;

    public AggregateStoreTests() => store = new AggregateStore<Counter, Incremented>(database);

    public record Incremented(int Value);

    public class Counter: Aggregate<Incremented, Guid>
    {
        [JsonInclude] public int Value { get; private set; }

        public static Counter Create(Guid id)
        {
            var counter = new Counter { Id = id };
            return counter.Increment();
        }

        public Counter Increment()
        {
            Enqueue(new Incremented(Value + 1));
            return this;
        }

        public override void Apply(Incremented @event) => Value = @event.Value;
    }
}
