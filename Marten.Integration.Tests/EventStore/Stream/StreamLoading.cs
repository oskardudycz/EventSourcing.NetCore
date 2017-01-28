using System;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamLoading : MartenTest
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

        private readonly Guid _taskId = Guid.NewGuid();

        private Guid GetExistingStreamId()
        {
            var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };
            var streamId = EventStore.StartStream<TaskList>(@event);
            Session.SaveChanges();

            return streamId;
        }


        [Fact]
        public void GivenExistingStream_WithOneEventWhenStreamIsLoaded_ThenItLoadsOneEvent()
        {
            //Given
            var streamId = GetExistingStreamId();

            //When
            var events = EventStore.FetchStream(streamId);

            //Then
            events.Count.Should().Be.EqualTo(1);
            events.First().Version.Should().Be.EqualTo(1);
        }

        [Fact]
        public void GivenExistingStreamWithOneEvent_WhenEventIsAppended_ThenItLoadsTwoEvents()
        {
            //Given
            var streamId = GetExistingStreamId();

            //When
            EventStore.Append(streamId, new TaskUpdated { TaskId = _taskId, Description = "New Description" });
            Session.SaveChanges();

            //Then
            var events = EventStore.FetchStream(streamId);

            events.Count.Should().Be.EqualTo(2);
            events.Last().Version.Should().Be.EqualTo(2);
        }
    }
}
