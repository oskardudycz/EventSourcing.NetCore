using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections
{
    public class OutOfOrderProjectionsTest: MartenTest
    {
        public interface ITaskEvent
        {
            Guid TaskId { get; set; }

            int TaskVersion { get; set; }
        }

        public class TaskCreated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }

            public int TaskVersion { get; set; }
        }

        public class TaskUpdated: ITaskEvent
        {
            public Guid TaskId { get; set; }
            public string Description { get; set; }

            public int TaskVersion { get; set; }
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
                var task = List.SingleOrDefault(t => t.TaskId == @event.TaskId);

                if (task == null)
                {
                    return;
                }

                task.Description = @event.Description;
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
            });

            return store.OpenSession();
        }

        [Fact]
        public void GivenOutOfOrderEvents_WhenPublishedWithSetVersion_ThenLiveAggregationWorksFine()
        {
            var firstTaskId = Guid.NewGuid();
            var secondTaskId = Guid.NewGuid();

            var events = new ITaskEvent[]
            {
                new TaskUpdated {TaskId = firstTaskId, Description = "Final First Task Description", TaskVersion = 4 },
                new TaskCreated {TaskId = firstTaskId, Description = "First Task", TaskVersion = 1 },
                new TaskCreated {TaskId = secondTaskId, Description = "Second Task 2", TaskVersion = 2 },
                new TaskUpdated {TaskId = firstTaskId, Description = "Intermediate First Task Description", TaskVersion = 3},
                new TaskUpdated {TaskId = secondTaskId, Description = "Final Second Task Description", TaskVersion = 5},
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
