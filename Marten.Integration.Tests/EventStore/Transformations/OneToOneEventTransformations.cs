using System;
using System.Linq;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Transformations
{
    public class OneToOneEventTransformations: MartenTest
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

        private enum ChangeType
        {
            Creation,
            Modification
        }

        private class TaskChangesLog
        {
            public Guid Id { get; set; }
            public Guid TaskId { get; set; }
            public ChangeType ChangeType { get; set; }
            public DateTime Timestamp { get; set; }
        }

        private class TaskList { }

        private class TaskChangeLogTransform: ITransform<TaskCreated, TaskChangesLog>,
            ITransform<TaskUpdated, TaskChangesLog>
        {
            public TaskChangesLog Transform(EventStream stream, Event<TaskCreated> input)
            {
                return new TaskChangesLog
                {
                    TaskId = input.Data.TaskId,
                    Timestamp = input.Timestamp.DateTime,
                    ChangeType = ChangeType.Creation
                };
            }

            public TaskChangesLog Transform(EventStream stream, Event<TaskUpdated> input)
            {
                return new TaskChangesLog
                {
                    TaskId = input.Data.TaskId,
                    Timestamp = input.Timestamp.DateTime,
                    ChangeType = ChangeType.Modification
                };
            }
        }

        protected override IDocumentSession CreateSession(Action<StoreOptions> setStoreOptions)
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
            EventStore.StartStream<TaskList>(events);

            Session.SaveChanges();

            //2. Get transformed events
            var changeLogs = Session.Query<TaskChangesLog>().ToList();

            changeLogs.Should().Have.Count.EqualTo(events.Length);

            changeLogs.Select(ev => ev.TaskId)
                .Should().Have.SameValuesAs(events.Select(ev => ev.TaskId));

            changeLogs.Count(ev => ev.ChangeType == ChangeType.Creation)
                .Should().Be.EqualTo(events.OfType<TaskCreated>().Count());

            changeLogs.Count(ev => ev.ChangeType == ChangeType.Modification)
                .Should().Be.EqualTo(events.OfType<TaskUpdated>().Count());

            changeLogs.Count(ev => ev.TaskId == task1Id)
                .Should().Be.EqualTo(events.Count(ev => ev.TaskId == task1Id));

            changeLogs.Count(ev => ev.TaskId == task2Id)
                .Should().Be.EqualTo(events.Count(ev => ev.TaskId == task2Id));
        }
    }
}
