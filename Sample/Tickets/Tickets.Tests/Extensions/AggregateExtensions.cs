using System.Linq;
using Core.Aggregates;
using Core.Events;

namespace Tickets.Tests.Extensions
{
    public static class AggregateExtensions
    {
        public static T? PublishedEvent<T>(this IAggregate aggregate) where T : class, IEvent
        {
            return aggregate.DequeueUncommittedEvents().LastOrDefault() as T;
        }
    }
}
