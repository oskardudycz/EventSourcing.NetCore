using System;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamLoading: MartenTest
    {
        private class TaskCreated
        {
            public Guid Id { get; set; }
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskUpdated
        {
            public Guid Id { get; set; }
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskList { }

        private readonly Guid _taskId = Guid.NewGuid();

        private Guid GetExistingStreamId()
        {
            var @event = new TaskCreated { TaskId = _taskId, Description = "Description" };
            var streamId = EventStore.StartStream<TaskList>(@event).Id;
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
            EventStore.Append(streamId, new TaskUpdated { Id = Guid.NewGuid(), TaskId = _taskId, Description = "New Description" });
            Session.SaveChanges();

            //Then
            var events = EventStore.FetchStream(streamId);

            events.Count.Should().Be.EqualTo(2);
            events.Last().Version.Should().Be.EqualTo(2);
        }

        [Fact]
        public void GivenExistingStreamWithOneEvent_WhenStreamIsLoadedByEventType_ThenItLoadsOneEvent()
        {
            //Given
            var streamId = GetExistingStreamId();
            var eventId = EventStore.FetchStream(streamId).Single().Id;

            //When
            var @event = EventStore.Load<TaskCreated>(eventId);

            //Then
            @event.Should().Not.Be.Null();
            @event.Id.Should().Be.EqualTo(eventId);
        }
    }
}
