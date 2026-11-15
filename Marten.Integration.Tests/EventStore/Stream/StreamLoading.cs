using Core.Testing.Fixtures;
using Shouldly;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public class StreamLoading(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
{
    public record IssueCreated(
        string IssueId,
        string Description
    );

    public record IssueUpdated(
        string IssueId,
        string Description
    );

    private readonly string issueId = GenerateRandomId();

    private async Task<string> GetExistingStreamId()
    {
        var streamId = GenerateRandomId();
        var @event = new IssueCreated(issueId, "Description");
        EventStore.StartStream(streamId, @event);
        await Session.SaveChangesAsync();

        return streamId;
    }

    [Fact]
    public async Task GivenExistingStream_WithOneEventWhenStreamIsLoaded_ThenItLoadsOneEvent()
    {
        //Given
        var streamId = await GetExistingStreamId();

        //When
        var events = await EventStore.FetchStreamAsync(streamId, token: TestContext.Current.CancellationToken);

        //Then
        events.Count.ShouldBe(1);
        events.First().Version.ShouldBe(1);
    }

    [Fact]
    public async Task GivenExistingStreamWithOneEvent_WhenEventIsAppended_ThenItLoadsTwoEvents()
    {
        //Given
        var streamId = await GetExistingStreamId();

        //When
        EventStore.Append(streamId, new IssueUpdated(issueId, "New Description"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        //Then
        var events = await EventStore.FetchStreamAsync(streamId, token: TestContext.Current.CancellationToken);

        events.Count.ShouldBe(2);
        events.Last().Version.ShouldBe(2);
    }

    [Fact]
    public async Task GivenExistingStreamWithOneEvent_WhenStreamIsLoadedByEventType_ThenItLoadsOneEvent()
    {
        //Given
        var streamId = await GetExistingStreamId();
        var eventId = (await EventStore.FetchStreamAsync(streamId, token: TestContext.Current.CancellationToken)).Single().Id;

        //When
        var @event = await EventStore.LoadAsync<IssueCreated>(eventId, TestContext.Current.CancellationToken);

        //Then
        @event.ShouldNotBeNull();
        @event!.Id.ShouldBe(eventId);
    }

    [Fact]
    public async Task
        GivenExistingStreamWithMultipleEvents_WhenEventsAreQueriedOrderedDescending_ThenLastEventIsLoaded()
    {
        var streamId = GenerateRandomId();
        Session.Events.Append(streamId,
            new IssueCreated(streamId, "Description"),
            new IssueUpdated(streamId, "Description"),
            new IssueUpdated(streamId, "The Last One")
        );
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var lastEvent = await Session.Events.QueryAllRawEvents()
            .Where(e => e.StreamKey == streamId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefaultAsync(token: TestContext.Current.CancellationToken);

        lastEvent.ShouldNotBeNull();
        lastEvent!.Data.ShouldBeOfType<IssueUpdated>();

        var lastUpdatedEvent = (IssueUpdated)lastEvent.Data;
        lastUpdatedEvent.IssueId.ShouldBe(streamId);
        lastUpdatedEvent.Description.ShouldBe("The Last One");
    }
}
