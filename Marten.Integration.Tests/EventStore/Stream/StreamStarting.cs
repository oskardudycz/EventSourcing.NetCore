using System;
using Marten.Exceptions;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamStarting: MartenTest
    {
        public class IssueCreated
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssueUpdated
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssuesList { }

        [Fact (Skip = "Skipped because of https://github.com/JasperFx/marten/issues/1648")]
        public void GivenNoEvents_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
        {
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<IssuesList>(@event.IssueId);

            Session.SaveChanges();

            streamId.Should().Not.Be.Null();
        }

        [Fact]
        public void GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
        {
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<IssuesList>(@event.IssueId, @event);

            streamId.Should().Not.Be.Null();

            Session.SaveChanges();
        }

        [Fact]
        public void GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
        {
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<IssuesList>(@event.IssueId, @event);

            Session.SaveChanges();

            Assert.Throws<ExistingStreamIdCollisionException>(() =>
            {
                EventStore.StartStream<IssuesList>(@event.IssueId, @event);
                Session.SaveChanges();
            });
        }

        [Fact]
        public void GivenOneEvent_WhenEventsArePublishedWithStreamId_ThenEventsAreSavedWithoutErrorAndStreamIsStarted()
        {
            var @event = new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description" };
            var streamId = Guid.NewGuid();

            EventStore.Append(streamId, @event);

            Session.SaveChanges();

            var streamState = EventStore.FetchStreamState(streamId);

            streamState.Should().Not.Be.Null();
            streamState.Version.Should().Be.EqualTo(1);
        }

        [Fact]
        public void GivenMoreThenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
        {
            var taskId = Guid.NewGuid();
            var events = new object[]
            {
                    new IssueCreated {IssueId = taskId, Description = "Description1"},
                    new IssueUpdated {IssueId = taskId, Description = "Description2"}
            };

            var streamId = EventStore.StartStream<IssuesList>(events);

            streamId.Should().Not.Be.Null();

            Session.SaveChanges();
        }
    }
}
