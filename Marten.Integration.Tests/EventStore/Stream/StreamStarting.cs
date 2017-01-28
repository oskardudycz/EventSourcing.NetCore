using System;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Stream
{
    public class StreamStarting : MartenTest
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
        public void GivenOneEvent_WhenStreamIsStarted_ThenEventsAreSavedWithouthError()
        {
            var ex = Record.Exception(() =>
            {
                var @event = new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description" };

                var streamId = EventStore.StartStream<TaskList>(@event);

                streamId.Should().Not.Be.Null();

                Session.SaveChanges();
            });

            ex.Should().Be.Null();
        }

        [Fact]
        public void GivenMoreThenOneEvent_WhenStreamIsStarted_ThenEventsAreSavedWithouthError()
        {
            var ex = Record.Exception(() =>
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
            });

            ex.Should().Be.Null();
        }
    }
}
