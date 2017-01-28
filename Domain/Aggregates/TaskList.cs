using System.Collections.Generic;
using System.Threading.Tasks;

namespace Domain.Tests.Marten.EventStore.Stubs.Aggregates
{
    public class TaskList
    {
        public List<Task> Tasks { get; set; }
    }
}
