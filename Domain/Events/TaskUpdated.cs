using System;

namespace Domain.Tests.Marten.EventStore.Stubs.Events
{
    public class TaskUpdated
    {
        public Guid TaskId { get; set; }
        public Guid Description { get; set; }
    }
}
