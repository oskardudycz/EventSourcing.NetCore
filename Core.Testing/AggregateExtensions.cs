using Core.Aggregates;
using Core.Events;

namespace Core.Testing;

public static class AggregateExtensions
{
    public static T? PublishedEvent<T>(this IAggregate aggregate) where T : class
    {
        return aggregate.DequeueUncommittedEvents().LastOrDefault() as T;
    }
}
