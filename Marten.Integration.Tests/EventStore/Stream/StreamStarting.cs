using System;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamStarting: MartenTest
    {
        private class TaskCreated
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskUpdated
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskList { }

        [Fact]
        public void GivenNoEvents_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
        {
            var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<TaskList>(@event.TaskId);

            Session.SaveChanges();

            streamId.Should().Not.Be.Null();
        }

        [Fact]
        public void GivenOneEvent_WhenStreamIsStarting_ThenEventsAreSavedWithoutError()
        {
            var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<TaskList>(@event.TaskId, @event);

            streamId.Should().Not.Be.Null();

            Session.SaveChanges();
        }

        [Fact]
        public void GivenStartedStream_WhenStartStreamIsBeingCalledAgain_ThenExceptionIsThrown()
        {
            var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };

            var streamId = EventStore.StartStream<TaskList>(@event.TaskId, @event);

            Session.SaveChanges();

            Assert.Throws<Events.ExistingStreamIdCollisionException>(() =>
            {
                EventStore.StartStream<TaskList>(@event.TaskId, @event);
                Session.SaveChanges();
            });
        }

        [Fact]
        public void GivenOneEvent_WhenEventsArePublishedWithStreamId_ThenEventsAreSavedWithoutErrorAndStreamIsStarted()
        {
            var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };
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
                    new TaskCreated {TaskId = taskId, Description = "Description1"},
                    new TaskUpdated {TaskId = taskId, Description = "Description2"}
            };

            var streamId = EventStore.StartStream<TaskList>(events);

            streamId.Should().Not.Be.Null();

            Session.SaveChanges();
        }
    }
}
