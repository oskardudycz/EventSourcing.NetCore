using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public class EventsAggregation: MartenTest
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

        private class TaskRemoved
        {
            public Guid TaskId { get; set; }
        }

        private class Task
        {
            public Guid TaskId { get; set; }

            public string Description { get; set; }
        }

        private class TaskList
        {
            public Guid Id { get; set; }
            public List<Task> List { get; private set; }

            public TaskList()
            {
                List = new List<Task>();
            }

            public void Apply(TaskCreated @event)
            {
                List.Add(new Task { TaskId = @event.TaskId, Description = @event.Description });
            }

            public void Apply(TaskUpdated @event)
            {
                var task = List.Single(t => t.TaskId == @event.TaskId);

                task.Description = @event.Description;
            }

            public void Apply(TaskRemoved @event)
            {
                var task = List.Single(t => t.TaskId == @event.TaskId);

                List.Remove(task);
            }
        }

        [Fact]
        public void GivenStreamOfEvents_WhenAggregateStreamIsCalled_ThenChangesAreAppliedProperly()
        {
            var streamId = EventStore.StartStream<TaskList>().Id;

            //1. First Task Was Created
            var task1Id = Guid.NewGuid();
            EventStore.Append(streamId, new TaskCreated { TaskId = task1Id, Description = "Description" });
            Session.SaveChanges();

            var taskList = EventStore.AggregateStream<TaskList>(streamId);

            taskList.List.Should().Have.Count.EqualTo(1);
            taskList.List.Single().TaskId.Should().Be.EqualTo(task1Id);
            taskList.List.Single().Description.Should().Be.EqualTo("Description");

            //2. First Task Description Was Changed
            EventStore.Append(streamId, new TaskUpdated { TaskId = task1Id, Description = "New Description" });
            Session.SaveChanges();

            taskList = EventStore.AggregateStream<TaskList>(streamId);

            taskList.List.Should().Have.Count.EqualTo(1);
            taskList.List.Single().TaskId.Should().Be.EqualTo(task1Id);
            taskList.List.Single().Description.Should().Be.EqualTo("New Description");

            //3. Two Other tasks were added
            EventStore.Append(streamId, new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description2" },
                new TaskCreated { TaskId = Guid.NewGuid(), Description = "Description3" });
            Session.SaveChanges();

            taskList = EventStore.AggregateStream<TaskList>(streamId);

            taskList.List.Should().Have.Count.EqualTo(3);
            taskList.List.Select(t => t.Description)
                .Should()
                .Have.SameSequenceAs("New Description", "Description2", "Description3");

            //4. First task was removed
            EventStore.Append(streamId, new TaskRemoved { TaskId = task1Id });
            Session.SaveChanges();

            taskList = EventStore.AggregateStream<TaskList>(streamId);

            taskList.List.Should().Have.Count.EqualTo(2);
            taskList.List.Select(t => t.Description)
                .Should()
                .Have.SameSequenceAs("Description2", "Description3");
        }
    }
}
