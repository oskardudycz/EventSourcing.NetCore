using System;

namespace Domain.Tests.Marten.EventStore.Stubs.Events
{
    public class TaskRemoved
    {
        public Guid TaskId { get; set; }
    }
}
