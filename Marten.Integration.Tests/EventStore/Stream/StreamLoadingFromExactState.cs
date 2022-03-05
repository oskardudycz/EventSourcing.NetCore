using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public class StreamLoadingFromExactState: MartenTest
{
    public record IssueCreated(
        Guid IssueId,
        string Description
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description
    );

    [Fact(Skip = "Skipping - AppVeyor for some reason doesn't like it -_-")]
    public void GivenSetOfEvents_WithFetchEventsFromDifferentTimes_ThenProperSetsAreLoaded()
    {
        //Given
        var streamId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        //When
        var beforeCreateTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueCreated(taskId, "Initial Name"));
        Session.SaveChanges();
        var createTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated name"));
        Session.SaveChanges();
        var firstUpdateTimestamp = DateTime.UtcNow;

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated again name"),
            new IssueUpdated(taskId, "Updated again and again name"));
        Session.SaveChanges();
        var secondUpdateTimestamp = DateTime.UtcNow;

        //Then
        var events = EventStore.FetchStream(streamId, timestamp: beforeCreateTimestamp);
        events.Count.Should().Be(0);

        events = EventStore.FetchStream(streamId, timestamp: createTimestamp);
        events.Count.Should().Be(1);

        events = EventStore.FetchStream(streamId, timestamp: firstUpdateTimestamp);
        events.Count.Should().Be(2);

        events = EventStore.FetchStream(streamId, timestamp: secondUpdateTimestamp);
        events.Count.Should().Be(4);
    }

    [Fact]
    public void GivenSetOfEvents_WithFetchEventsFromDifferentVersionNumber_ThenProperSetsAreLoaded()
    {
        //Given
        var streamId = Guid.NewGuid();
        var taskId = Guid.NewGuid();

        //When
        EventStore.Append(streamId, new IssueCreated(taskId, "Initial Name"));
        Session.SaveChanges();

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated name"));
        Session.SaveChanges();

        EventStore.Append(streamId, new IssueUpdated(taskId, "Updated again name"),
            new IssueUpdated(taskId, "Updated again and again name"));
        Session.SaveChanges();

        //Then
        //version after create
        var events = EventStore.FetchStream(streamId, 1);
        events.Count.Should().Be(1);

        //version after first update
        events = EventStore.FetchStream(streamId, 2);
        events.Count.Should().Be(2);

        //even though 3 and 4 updates were append at the same time version is incremented for both of them
        events = EventStore.FetchStream(streamId, 3);
        events.Count.Should().Be(3);

        events = EventStore.FetchStream(streamId, 4);
        events.Count.Should().Be(4);

        //fetching with version equal to 0 returns the most recent state
        events = EventStore.FetchStream(streamId, 0);
        events.Count.Should().Be(4);

        //providing bigger version than current doesn't throws exception - returns most recent state
        events = EventStore.FetchStream(streamId, 100);
        events.Count.Should().Be(4);
    }
}
