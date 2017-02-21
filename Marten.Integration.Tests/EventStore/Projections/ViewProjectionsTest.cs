using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;
using Marten.Events.Projections;

namespace Marten.Integration.Tests.EventStore.Projections
{
    public class ViewProjectionsTest : MartenTest
    {
        private interface ITaskEvent
        {
            Guid TaskId { get; set; }
        }

        private class TaskCreated : ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        private class TaskUpdated : ITaskEvent
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

        private class TaskDescriptionView
        {
            public Guid Id { get; set; }
            public IDictionary<Guid, string> Descriptions { get; } = new Dictionary<Guid, string>();

            internal void ApplyEvent(TaskCreated @event)
            {
                Descriptions.Add(@event.TaskId, @event.Description);
            }

            internal void ApplyEvent(TaskUpdated @event)
            {
                Descriptions[@event.TaskId] = @event.Description;
            }
        }
        
        private class TaskListViewProjection : ViewProjection<TaskDescriptionView>
        {
            readonly Guid viewid = new Guid("a8c1a4ac-686d-4fb7-a64a-710bc630f471");
            public TaskListViewProjection()
            {
                ProjectEvent<TaskCreated>((ev) => viewid, Persist);
                ProjectEvent<TaskUpdated>((ev) => viewid, Persist);
            }

            private void Persist(TaskDescriptionView view, TaskCreated @event)
            {
                view.ApplyEvent(@event);
            }

            private void Persist(TaskDescriptionView view, TaskUpdated @event)
            {
                view.ApplyEvent(@event);
            }
        }



        protected override IDocumentSession CreateSession()
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(Settings.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = SchemaName;
                options.Events.DatabaseSchemaName = SchemaName;

                //It's needed to manualy set that inline aggegation should be applied
                options.Events.InlineProjections.AggregateStreamsWith<TaskList>();
                options.Events.InlineProjections.Add(new TaskListViewProjection());
            });

            return store.OpenSession();
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
            var streamId = EventStore.StartStream<TaskList>(events);

            Session.SaveChanges();

            //2. Get live agregation
            var taskListFromLiveAggregation = EventStore.AggregateStream<TaskList>(streamId);

            //3. Get inline aggregation
            var taskListFromInlineAggregation = Session.Load<TaskList>(streamId);

            var projection = Session.Query<TaskDescriptionView>().FirstOrDefault();

            taskListFromLiveAggregation.Should().Not.Be.Null();
            taskListFromInlineAggregation.Should().Not.Be.Null();

            taskListFromLiveAggregation.List.Count.Should().Be.EqualTo(2);
            taskListFromLiveAggregation.List.Count.Should().Be.EqualTo(taskListFromInlineAggregation.List.Count);
        }
    }
}
