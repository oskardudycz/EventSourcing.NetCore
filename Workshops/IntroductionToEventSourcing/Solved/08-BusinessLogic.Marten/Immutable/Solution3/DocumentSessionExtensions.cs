using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Immutable.Solution3;

public static class DocumentSessionExtensions
{
    public static async Task<TEntity> Get<TEntity>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TEntity : class
    {
        var entity = await session.Events.AggregateStreamAsync<TEntity>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static Task Decide<TEntity, TCommand, TEvent>(
        this IDocumentSession session,
        Func<TCommand, TEntity, TEvent[]> decide,
        Func<TEntity> getDefault,
        Guid streamId,
        TCommand command,
        CancellationToken ct = default
    ) where TEntity : class =>
        session.Events.WriteToAggregate<TEntity>(streamId, stream =>
            stream.AppendMany(decide(command, stream.Aggregate ?? getDefault()).Cast<object>().ToArray()), ct);
}
