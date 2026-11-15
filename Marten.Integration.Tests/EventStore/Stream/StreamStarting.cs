using Core.Testing.Fixtures;
using Shouldly;
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

public class StreamStarting(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
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

        await starting.ShouldThrowAsync<EmptyEventStreamException>();
    }

    [Fact]
    public Task GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");

        var streamId = EventStore.StartStream(@event.IssueId, @event);

        streamId.ShouldNotBeNull();

        return Session.SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact]
    public async Task GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");

        EventStore.StartStream(@event.IssueId, @event);

        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        await Assert.ThrowsAsync<Marten.Exceptions.ExistingStreamIdCollisionException>(async () =>
        {
            EventStore.StartStream(@event.IssueId, @event);
            await Session.SaveChangesAsync(TestContext.Current.CancellationToken);
        });
    }

    [Fact]
    public async Task GivenOneEvent_WhenEventsArePublishedWithStreamId_ThenEventsAreSavedWithoutErrorAndStreamIsStarted()
    {
        var @event = new IssueCreated(GenerateRandomId(), "Description");
        var streamId = GenerateRandomId();

        EventStore.Append(streamId, @event);

        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var streamState = await EventStore.FetchStreamStateAsync(streamId, TestContext.Current.CancellationToken);

        streamState.ShouldNotBeNull();
        streamState!.Version.ShouldBe(1);
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

        streamId.ShouldNotBeNull();

        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);
    }
}
