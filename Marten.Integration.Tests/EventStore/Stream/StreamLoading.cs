using System;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamLoading: MartenTest
    {
        public class IssueCreated
        {
            public Guid Id { get; set; }
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssueUpdated
        {
            public Guid Id { get; set; }
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssuesList { }

        private readonly Guid issueId = Guid.NewGuid();

        private Guid GetExistingStreamId()
        {
            var @event = new IssueCreated { IssueId = issueId, Description = "Description" };
            var streamId = EventStore.StartStream<IssuesList>(@event).Id;
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
            EventStore.Append(streamId, new IssueUpdated { Id = Guid.NewGuid(), IssueId = issueId, Description = "New Description" });
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
            var @event = EventStore.Load<IssueCreated>(eventId);

            //Then
            @event.Should().Not.Be.Null();
            @event.Id.Should().Be.EqualTo(eventId);
        }
    }
}
