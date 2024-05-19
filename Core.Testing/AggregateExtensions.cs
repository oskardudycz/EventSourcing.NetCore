using Core.Aggregates;

namespace Core.Testing;

public static class AggregateExtensions
{
    public static T? PublishedEvent<T>(this IAggregate aggregate) where T : class =>
        aggregate.DequeueUncommittedEvents().LastOrDefault() as T;
}
