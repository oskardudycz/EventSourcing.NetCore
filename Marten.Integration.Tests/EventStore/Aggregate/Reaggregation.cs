using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using System;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    class TaskCreated
    {
        public Guid TaskId { get; set; }
        public string Description { get; set; }
    }

    class TaskUpdated
    {
        public Guid TaskId { get; set; }
        public string Description { get; set; }
    }

    namespace OldVersion
    {
        class Task
        {
            public Guid Id { get; set; }

            public string Description { get; set; }

            public void Apply(TaskCreated @event)
            {
                Id = @event.TaskId;
                Description = @event.Description;
            }

            public void Apply(TaskUpdated @event)
            {
                Description = @event.Description;
            }
        }
    }    

    namespace NewVersion
    {
        class Task
        {
            public Guid Id { get; set; }

            public string Description { get; set; }

            public void Apply(TaskCreated @event)
            {
                Id = @event.TaskId;
                Description = $"Description: {@event.Description}";
            }

            public void Apply(TaskUpdated @event)
            {
                Description = $"Description: {@event.Description}";
            }
        }
    }

    public class Reaggregation : MartenTest
    {
        public Reaggregation() : base(false) { }

        public IDocumentSession CreateSessionWithInlineAggregationFor<TTask>() where TTask : class, new ()
        {
            return base.CreateSession(options =>
            {
                options.Events.AddEventTypes(new[] { typeof(TaskCreated), typeof(TaskUpdated) });
                //It's needed to manualy set that inline aggegation should be applied
                options.Events.InlineProjections.AggregateStreamsWith<TTask>();
            });
        }

        [Fact]
        public void Given_When_Then()
        {
            var taskId = Guid.NewGuid();

            var events = new object[]
            {
                new TaskCreated {TaskId = taskId, Description = "Task 1"},
                new TaskUpdated {TaskId = taskId, Description = "Task 1 Updated"},
            };
            
            OldVersion.Task taskFromV1InlineAggregation;
            OldVersion.Task taskFromV1OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<OldVersion.Task>())
            {
                //1. Publish events
                session.Events.StartStream<OldVersion.Task>(taskId, events);

                session.SaveChanges();

                taskFromV1InlineAggregation = session.Load<OldVersion.Task>(taskId);
                taskFromV1OnlineAggregation = session.Events.AggregateStream<OldVersion.Task>(taskId);
            }

            //2. Both inline and online aggregation for the same type should be the same
            taskFromV1InlineAggregation.Description.Should().Be.EqualTo("Task 1 Updated");
            taskFromV1InlineAggregation.Description.Should().Be.EqualTo(taskFromV1OnlineAggregation.Description);

            //3. Simulate change to aggregation logic
            NewVersion.Task taskFromV2InlineAggregation;
            NewVersion.Task taskFromV2OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Task>())
            {
                taskFromV2InlineAggregation = session.Load<NewVersion.Task>(taskId);
                taskFromV2OnlineAggregation = session.Events.AggregateStream<NewVersion.Task>(taskId);
            }

            //4. Inline aggregated snapshot won't change automatically
            taskFromV2InlineAggregation.Description.Should().Be.EqualTo(taskFromV1InlineAggregation.Description);
            taskFromV2InlineAggregation.Description.Should().Not.Be.EqualTo("Description: Task 1 Updated");
            
            //5. But online aggregation is being applied automatically
            taskFromV2OnlineAggregation.Description.Should().Not.Be.EqualTo(taskFromV1OnlineAggregation.Description);
            taskFromV2OnlineAggregation.Description.Should().Be.EqualTo("Description: Task 1 Updated");

            //6. Reagregation
            using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Task>())
            {
                //7. Get online aggregation
                //8. Store manually aggregation as reaggregated inline aggregation
                session.Store(taskFromV2OnlineAggregation);
                session.SaveChanges();
                
                var taskFromV2AfterReaggregation = session.Load<NewVersion.Task>(taskId);

                taskFromV2AfterReaggregation.Description.Should().Not.Be.EqualTo(taskFromV1OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.Should().Be.EqualTo(taskFromV2OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.Should().Be.EqualTo("Description: Task 1 Updated");

                //9. Check if next event would be properly applied to inline aggregation
                session.Events.Append(taskId, new TaskUpdated { TaskId = taskId, Description = "Completely New text" });
                session.SaveChanges();

                var taskFromV2NewInlineAggregation = session.Load<NewVersion.Task>(taskId);
                taskFromV2NewInlineAggregation.Description.Should().Be.EqualTo("Description: Completely New text");
            }
        }
    }
}
