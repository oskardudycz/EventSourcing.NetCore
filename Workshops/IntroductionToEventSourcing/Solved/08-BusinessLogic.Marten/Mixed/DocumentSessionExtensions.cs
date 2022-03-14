using Marten;

namespace IntroductionToEventSourcing.BusinessLogic.Mixed;

public static class DocumentSessionExtensions
{
    public static async Task<TAggregate> Get<TAggregate>(
        this IDocumentSession session,
        Guid id,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        var entity = await session.Events.AggregateStreamAsync<TAggregate>(id, token: cancellationToken);

        return entity ?? throw new InvalidOperationException($"Entity with id {id} was not found");
    }

    public static Task Add<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        session.Events.StartStream<TAggregate>(
            getId(command),
            action(command)
        );

        return session.SaveChangesAsync(cancellationToken);
    }

    public static async Task GetAndUpdate<TAggregate, TCommand>(
        this IDocumentSession session,
        Func<TCommand, Guid> getId,
        Func<TCommand, TAggregate, object> action,
        TCommand command,
        CancellationToken cancellationToken = default
    ) where TAggregate : class, IAggregate
    {
        var id = getId(command);
        var current = await session.Get<TAggregate>(id, cancellationToken);

        session.Events.Append(id, action(command, current));

        await session.SaveChangesAsync(cancellationToken);
    }
}
