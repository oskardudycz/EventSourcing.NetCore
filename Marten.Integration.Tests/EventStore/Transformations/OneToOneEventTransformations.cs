using System;
using System.Linq;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Transformations
{
    public class OneToOneEventTransformations : MartenTest
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

        private class TaskChangesLog
        {
            public Guid Id { get; set; }
            public Guid TaskId { get; set; }

            public DateTime Timestamp { get; set; }
        }

        private class TaskList {}

        private class TaskChangeLogTransform : ITransform<TaskCreated, TaskChangesLog>,
            ITransform<TaskUpdated, TaskChangesLog>
        {
            public TaskChangesLog Transform(EventStream stream, Event<TaskCreated> input)
            {
                return new TaskChangesLog
                {
                    TaskId = input.Data.TaskId,
                    Timestamp = DateTime.Now
                };
            }

            public TaskChangesLog Transform(EventStream stream, Event<TaskUpdated> input)
            {
                return new TaskChangesLog
                {
                    TaskId = input.Data.TaskId,
                    Timestamp = DateTime.Now
                };
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

                //It's needed to manualy set that transformations should be applied
                options.Events.InlineProjections.TransformEvents<TaskCreated, TaskChangesLog>(new TaskChangeLogTransform());
                options.Events.InlineProjections.TransformEvents<TaskUpdated, TaskChangesLog>(new TaskChangeLogTransform());
            });

            return store.OpenSession();
        }

        [Fact]
        public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
        {
            var taskIds = new[]
            {
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid()
            };

            var events = new object[]
            {
                new TaskCreated {TaskId = taskIds[0], Description = "Description1"},
                new TaskCreated {TaskId = taskIds[1], Description = "Description2"},
                new TaskCreated {TaskId = taskIds[2], Description = "Description3"},

                new TaskUpdated {TaskId = taskIds[0], Description = "Description1New"},
                new TaskUpdated {TaskId = taskIds[1], Description = "Description2New"},
            };
            //1. Create events
            EventStore.StartStream<TaskList>(events);

            Session.SaveChanges();

            //2. Get transformed events
            var transformedEvents = Session.Query<TaskChangesLog>().ToList();

            transformedEvents.Should().Have.Count.EqualTo(events.Length);

            var transFormedEventId = transformedEvents.Select(ev => ev.TaskId).OrderBy(id => id);
            var eventIds = events.OfType<TaskCreated>().Select(el => el.TaskId).Concat(events.OfType<TaskUpdated>().Select(el => el.TaskId)).OrderBy(id => id);

            transFormedEventId.Should().Have.SameSequenceAs(eventIds);
        }
    }
}
