using FluentAssertions;
using Xunit;
using Database = BusinessProcesses.Core.Database;

namespace BusinessProcesses.ToDoList.Core;

public class ProjectionTests
{
    public record Counter(Guid Id, int Count);

    public record Started(Guid CounterId);

    public record Incremented(Guid? CounterId);

    public record Finished(Guid CounterId);

    public class CounterProjection(Database database): Projection<Counter>(database)
    {
        public override Type[] CanHandle() => [typeof(Started), typeof(Incremented), typeof(Finished)];

        protected override Guid? GetDocumentId(object @event) =>
            @event switch
            {
                Started started => started.CounterId,
                Incremented incremented => incremented.CounterId,
                Finished finished => finished.CounterId,
                _ => null
            };

        protected override Counter? Evolve(Counter? current, object @event) =>
            @event switch
            {
                Started started => new Counter(started.CounterId, 0),
                Incremented => current! with { Count = current.Count + 1 },
                Finished => null,
                _ => current
            };
    }

    [Fact]
    public async Task FirstEvent_StoresAndReturnsTheDocument()
    {
        var id = Guid.NewGuid();

        var result = await projection.Handle(new Started(id));

        result.Should().Be(new Counter(id, 0));
        (await database.Get<Counter>(id, default)).Should().Be(new Counter(id, 0));
    }

    [Fact]
    public async Task NextEvent_EvolvesTheStoredDocument()
    {
        var id = Guid.NewGuid();
        await projection.Handle(new Started(id));

        var result = await projection.Handle(new Incremented(id));

        result.Should().Be(new Counter(id, 1));
        (await database.Get<Counter>(id, default)).Should().Be(new Counter(id, 1));
    }

    [Fact]
    public async Task EventWithoutDocumentId_IsSkipped()
    {
        var result = await projection.Handle(new Incremented(null));

        result.Should().BeNull();
    }

    [Fact]
    public async Task NoDocumentFromEvolve_DeletesTheDocument()
    {
        var id = Guid.NewGuid();
        await projection.Handle(new Started(id));

        var result = await projection.Handle(new Finished(id));

        result.Should().BeNull();
        (await database.Get<Counter>(id, default)).Should().BeNull();
    }

    [Fact]
    public void CanHandle_ListsTheProjectedEvents() =>
        projection.CanHandle().Should().Equal(typeof(Started), typeof(Incremented), typeof(Finished));

    private readonly Database database = new();
    private readonly CounterProjection projection;

    public ProjectionTests() =>
        projection = new CounterProjection(database);
}
