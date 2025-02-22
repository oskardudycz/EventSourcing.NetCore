using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public class StreamLoading(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
{
    public record IssueCreated(
        Guid IssueId,
        string Description
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description
    );

    private readonly Guid issueId = Guid.NewGuid();

    private async Task<Guid> GetExistingStreamId()
    {
        var @event = new IssueCreated(issueId, "Description");
        var streamId = EventStore.StartStream(@event).Id;
        await Session.SaveChangesAsync();

        return streamId;
    }

    [Fact]
    public async Task GivenExistingStream_WithOneEventWhenStreamIsLoaded_ThenItLoadsOneEvent()
    {
        //Given
        var streamId = await GetExistingStreamId();

        //When
        var events = await EventStore.FetchStreamAsync(streamId);

        //Then
        events.Count.Should().Be(1);
        events.First().Version.Should().Be(1);
    }

    [Fact]
    public async Task GivenExistingStreamWithOneEvent_WhenEventIsAppended_ThenItLoadsTwoEvents()
    {
        //Given
        var streamId = await GetExistingStreamId();

        //When
        EventStore.Append(streamId, new IssueUpdated(issueId, "New Description"));
        await Session.SaveChangesAsync();

        //Then
        var events = await EventStore.FetchStreamAsync(streamId);

        events.Count.Should().Be(2);
        events.Last().Version.Should().Be(2);
    }

    [Fact]
    public async Task GivenExistingStreamWithOneEvent_WhenStreamIsLoadedByEventType_ThenItLoadsOneEvent()
    {
        //Given
        var streamId = await GetExistingStreamId();
        var eventId = (await EventStore.FetchStreamAsync(streamId)).Single().Id;

        //When
        var @event = await EventStore.LoadAsync<IssueCreated>(eventId);

        //Then
        @event.Should().NotBeNull();
        @event!.Id.Should().Be(eventId);
    }

    [Fact]
    public async Task
        GivenExistingStreamWithMultipleEvents_WhenEventsAreQueriedOrderedDescending_ThenLastEventIsLoaded()
    {
        var streamId = Guid.NewGuid();
        Session.Events.Append(streamId,
            new IssueCreated(streamId, "Description"),
            new IssueUpdated(streamId, "Description"),
            new IssueUpdated(streamId, "The Last One")
        );
        await Session.SaveChangesAsync();

        var lastEvent = Session.Events.QueryAllRawEvents()
            .Where(e => e.StreamId == streamId)
            .OrderByDescending(e => e.Version)
            .FirstOrDefault();

        lastEvent.Should().NotBeNull();
        lastEvent!.Data.Should().BeOfType<IssueUpdated>();

        var lastUpdatedEvent = (IssueUpdated)lastEvent.Data;
        lastUpdatedEvent.IssueId.Should().Be(streamId);
        lastUpdatedEvent.Description.Should().Be("The Last One");
    }
}
