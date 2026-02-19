using FluentAssertions;
using JasperFx.Events;
using Marten.Exceptions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream;

public record IssueCreated(
    string IssueId,
    string Description
);

public record IssueUpdated(
    string IssueId,
    string Description
);

public record Issue(
    string IssueId,
    string Description
);

public class StreamStarting(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
{
    [Fact]
    public async Task GivenNoEvents_WhenStreamIsStarting_ThenEventsAreNotSavedWithError()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");

        var starting = (async () =>
        {
            EventStore.StartStream(@event.IssueId);

            await Session.SaveChangesAsync();
        });

        await starting.Should().ThrowAsync<EmptyEventStreamException>();
    }

    [Fact]
    public Task GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        streamId.Should().NotBeNull();

        return Session.SaveChangesAsync();
    }

    [Fact]
    public async Task GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");

        EventStore.StartStream(@event.IssueId, @event);

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
        var @event = new IssueCreated(GenerateRandomId(), "Description");
        var streamId = GenerateRandomId();

        EventStore.Append(streamId, @event);

        await Session.SaveChangesAsync();

        var streamState = await EventStore.FetchStreamStateAsync(streamId);

        streamState.Should().NotBeNull();
        streamState!.Version.Should().Be(1);
    }

    [Fact]
    public async Task GivenMoreThenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var streamId = GenerateRandomId();
        var taskId = GenerateRandomId();
        var events = new object[]
        {
            new IssueCreated(taskId, "Description1"), new IssueUpdated(taskId, "Description2")
        };

        EventStore.StartStream(streamId, events);

        streamId.Should().NotBeNull();

        await Session.SaveChangesAsync();
    }
}
