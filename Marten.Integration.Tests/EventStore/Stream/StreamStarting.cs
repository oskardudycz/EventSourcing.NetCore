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

public class StreamStarting(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
{
    [Fact(Skip = "Skipped because of https://github.com/JasperFx/marten/issues/1648")]
    public async Task GivenNoEvents_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId);

        await Session.SaveChangesAsync();

        streamId.Should().NotBeNull();
    }

    [Fact]
    public Task GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        streamId.Should().NotBeNull();

        return Session.SaveChangesAsync();
    }

    [Fact]
    public async Task GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        await Session.SaveChangesAsync();

        await Assert.ThrowsAsync<ExistingStreamIdCollisionException>(async () =>
        {
            EventStore.StartStream(@event.IssueId, @event);
            await Session.SaveChangesAsync();
        });
    }

    [Fact]
    public async Task GivenOneEvent_WhenEventsArePublishedWithStreamId_ThenEventsAreSavedWithoutErrorAndStreamIsStarted()
    {
        var @event = new IssueCreated(Guid.NewGuid(), "Description");
        var streamId = Guid.NewGuid();

        EventStore.Append(streamId, @event);

        await Session.SaveChangesAsync();

        var streamState = await EventStore.FetchStreamStateAsync(streamId);

        streamState.Should().NotBeNull();
        streamState!.Version.Should().Be(1);
    }

    [Fact]
    public async Task GivenMoreThenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var taskId = Guid.NewGuid();
        var events = new object[]
        {
            new IssueCreated(taskId, "Description1"), new IssueUpdated(taskId, "Description2")
        };

        var streamId = EventStore.StartStream(events);

        streamId.Should().NotBeNull();

        await Session.SaveChangesAsync();
    }
}
