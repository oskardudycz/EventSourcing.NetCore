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

    namespace V1
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

    namespace V2
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
            
            V1.Task taskFromV1InlineAggregation;
            V1.Task taskFromV1OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<V1.Task>())
            {
                //1. Publish events
                session.Events.StartStream<V1.Task>(taskId, events);

                session.SaveChanges();

                taskFromV1InlineAggregation = session.Load<V1.Task>(taskId);
                taskFromV1OnlineAggregation = session.Events.AggregateStream<V1.Task>(taskId);
            }

            //2. Simulate change to aggregation logic
            V2.Task taskFromV2InlineAggregation;
            V2.Task taskFromV2OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<V2.Task>())
            {
                taskFromV2InlineAggregation = session.Load<V2.Task>(taskId);
                taskFromV2OnlineAggregation = session.Events.AggregateStream<V2.Task>(taskId);
            }

            taskFromV1InlineAggregation.Description.Should().Be.EqualTo("Task 1 Updated");
            //3. Both inline and online aggregation for the same type should be the same
            taskFromV1InlineAggregation.Description.Should().Be.EqualTo(taskFromV1OnlineAggregation.Description);


            //4. Inline aggregated snapshot won't change automatically
            taskFromV1InlineAggregation.Description.Should().Be.EqualTo(taskFromV2InlineAggregation.Description);


            //5. But online aggregation is being applied automatically
            taskFromV1OnlineAggregation.Description.Should().Not.Be.EqualTo(taskFromV2OnlineAggregation.Description);
            taskFromV1OnlineAggregation.Description.Should().Not.Be.EqualTo("Description: Task 1 Updated");
        }
    }
}
