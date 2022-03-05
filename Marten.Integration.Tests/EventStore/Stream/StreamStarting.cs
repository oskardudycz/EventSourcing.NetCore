using FluentAssertions;
using Marten.Exceptions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public record IssueCreated(
    Guid IssueId,
    string Description
);

public record IssueUpdated(
    Guid IssueId,
    string Description
);

public record Issue(
    Guid IssueId,
    string Description
);

public class StreamStarting: MartenTest
{
    [Fact (Skip = "Skipped because of https://github.com/JasperFx/marten/issues/1648")]
    public void GivenNoEvents_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId);

        Session.SaveChanges();

        streamId.Should().NotBeNull();
    }

    [Fact]
    public void GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        streamId.Should().NotBeNull();

        Session.SaveChanges();
    }

    [Fact]
    public void GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        Session.SaveChanges();

        Assert.Throws<ExistingStreamIdCollisionException>(() =>
        {
            EventStore.StartStream(@event.IssueId, @event);
            Session.SaveChanges();
        });
    }

    [Fact]
    public void GivenOneEvent_WhenEventsArePublishedWithStreamId_ThenEventsAreSavedWithoutErrorAndStreamIsStarted()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");
        var streamId = Guid.NewGuid();

        EventStore.Append(streamId, @event);

        Session.SaveChanges();

        var streamState = EventStore.FetchStreamState(streamId);

        streamState.Should().NotBeNull();
        streamState.Version.Should().Be(1);
    }

    [Fact]
    public void GivenMoreThenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var taskId = Guid.NewGuid();
        var events = new object[]
        {
            new IssueCreated(taskId, "Description1"),
            new IssueUpdated(taskId, "Description2")
        };

        var streamId = EventStore.StartStream(events);

        streamId.Should().NotBeNull();

        Session.SaveChanges();
    }
}
