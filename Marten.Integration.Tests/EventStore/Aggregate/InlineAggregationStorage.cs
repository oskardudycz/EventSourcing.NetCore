using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public class InlineAggregationStorage
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
    }
}
