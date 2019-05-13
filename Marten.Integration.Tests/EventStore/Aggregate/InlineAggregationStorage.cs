using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public class InlineAggregationStorage: MartenTest
    {
        private interface ITaskEvent
        {
            Guid TaskId { get; set; }
        }

        private class TaskCreated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskUpdated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
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
        }

        protected override IDocumentSession CreateSession(Action<StoreOptions> storeOptions = null)
        {
            return base.CreateSession(options =>
            {
                //It's needed to manualy set that inline aggegation should be applied
                options.Events.InlineProjections.AggregateStreamsWith<TaskList>();
            });
        }

        [Fact]
        public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
        {
            var task1Id = Guid.NewGuid();
            var task2Id = Guid.NewGuid();

            var events = new ITaskEvent[]
            {
                new TaskCreated {TaskId = task1Id, Description = "Description 1"},
                new TaskUpdated {TaskId = task1Id, Description = "Description 1 New"},
                new TaskCreated {TaskId = task2Id, Description = "Description 2"},
                new TaskUpdated {TaskId = task1Id, Description = "Description 1 Super New"},
                new TaskUpdated {TaskId = task2Id, Description = "Description 2 New"},
            };

            //1. Create events
            var streamId = EventStore.StartStream<TaskList>(events).Id;

            Session.SaveChanges();

            //2. Get live agregation
            var taskListFromLiveAggregation = EventStore.AggregateStream<TaskList>(streamId);

            //3. Get inline aggregation
            var taskListFromInlineAggregation = Session.Load<TaskList>(streamId);

            taskListFromLiveAggregation.Should().Not.Be.Null();
            taskListFromInlineAggregation.Should().Not.Be.Null();

            taskListFromLiveAggregation.List.Count.Should().Be.EqualTo(2);
            taskListFromLiveAggregation.List.Count.Should().Be.EqualTo(taskListFromInlineAggregation.List.Count);
        }
    }
}
