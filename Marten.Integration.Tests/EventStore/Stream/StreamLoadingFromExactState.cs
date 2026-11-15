using Core.Testing.Fixtures;
using Shouldly;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public class StreamLoadingFromExactState(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
{
    public record IssueCreated(
        string IssueId,
        string Description
    );

    public record IssueUpdated(
        string IssueId,
        string Description
    );

    [Fact]
    public async Task GivenSetOfEvents_WithFetchEventsFromDifferentTimes_ThenProperSetsAreLoaded()
    {
        //Given
        var streamId = GenerateRandomId();
        var taskId = GenerateRandomId();

        //When
        var beforeCreateTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueCreated(taskId, "Initial Name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);
        var createTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);
        var firstUpdateTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated again name"),
            new IssueUpdated(taskId, "Updated again and again name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);
        var secondUpdateTimestamp = DateTime.UtcNow;

        //Then
        var events = await EventStore.FetchStreamAsync(streamId, timestamp: beforeCreateTimestamp, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(0);

        events = await EventStore.FetchStreamAsync(streamId, timestamp: createTimestamp, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(1);

        events = await EventStore.FetchStreamAsync(streamId, timestamp: firstUpdateTimestamp, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(2);

        events = await EventStore.FetchStreamAsync(streamId, timestamp: secondUpdateTimestamp, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(4);
    }

    [Fact]
    public async Task GivenSetOfEvents_WithFetchEventsFromDifferentVersionNumber_ThenProperSetsAreLoaded()
    {
        //Given
        var streamId = GenerateRandomId();
        var taskId = GenerateRandomId();

        //When
        EventStore.Append(streamId, new IssueCreated(taskId, "Initial Name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated again name"),
            new IssueUpdated(taskId, "Updated again and again name"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Then
        //version after create
        var events = await EventStore.FetchStreamAsync(streamId, 1, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(1);

        //version after first update
        events = await EventStore.FetchStreamAsync(streamId, 2, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(2);

        //even though 3 and 4 updates were append at the same time version is incremented for both of them
        events = await EventStore.FetchStreamAsync(streamId, 3, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(3);

        events = await EventStore.FetchStreamAsync(streamId, 4, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(4);

        //fetching with version equal to 0 returns the most recent state
        events = await EventStore.FetchStreamAsync(streamId, 0, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(4);

        //providing bigger version than current doesn't throws exception - returns most recent state
        events = await EventStore.FetchStreamAsync(streamId, 100, token: TestContext.Current.CancellationToken);
        events.Count.ShouldBe(4);
    }
}
