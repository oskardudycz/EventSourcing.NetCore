using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections
{
    public class ViewProjectionsTest: MartenTest
    {
        public interface ITaskEvent
        {
            Guid TaskId { get; set; }
        }

        public class TaskCreated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        public class TaskUpdated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }
        }

        public class Task
        {
            public Guid TaskId { get; set; }

            public string Description { get; set; }
        }

        public class TaskList
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

        public class TaskDescriptionView
        {
            public Guid Id { get; set; }
            public IDictionary<Guid, string> Descriptions { get; } = new Dictionary<Guid, string>();

            public void ApplyEvent(TaskCreated @event)
            {
                Descriptions.Add(@event.TaskId, @event.Description);
            }

            public void ApplyEvent(TaskUpdated @event)
            {
                Descriptions[@event.TaskId] = @event.Description;
            }
        }

        public class TaskListViewProjection: ViewProjection<TaskDescriptionView, Guid>
        {
            public TaskListViewProjection()
            {
                ProjectEventToSingleRecord<TaskCreated>((view, @event) => view.ApplyEvent(@event));
                ProjectEventToSingleRecord<TaskUpdated>((view, @event) => view.ApplyEvent(@event));
            }

            private ViewProjection<TaskDescriptionView, Guid> ProjectEventToSingleRecord<TEvent>(Action<TaskDescriptionView, TEvent> handler) where TEvent : class
            {
                return ProjectEvent((documentSession, ev) => FindIdOfRecord(documentSession) ?? Guid.NewGuid(), handler);
            }

            private Guid? FindIdOfRecord(IDocumentSession documentSession)
            {
                return documentSession.Query<TaskDescriptionView>()
                                   .Select(t => (Guid?)t.Id).SingleOrDefault();
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
            var streamId = EventStore.StartStream<TaskList>(events).Id;

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
