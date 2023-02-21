using Core.Exceptions;
using Core.ProcessManagers;
using Marten;

namespace Core.Marten.ProcessManagers;

/// <summary>
/// This code assumes that Process Manager:
/// - is event-driven but not fully event-sourced
/// - streams have string identifiers
/// - process manager is versioned, so optimistic concurrency is applied
/// </summary>
public static class DocumentSessionExtensions
{
    public static Task Add<T>(
        this IDocumentSession documentSession,
        string id,
        T processManager,
        CancellationToken ct
    ) where T : IProcessManager
    {
        documentSession.Insert(processManager);
        EnqueueMessages(documentSession, id, processManager);

        return documentSession.SaveChangesAsync(token: ct);
    }

    public static async Task GetAndUpdate<T>(
        this IDocumentSession documentSession,
        string id,
        Action<T> handle,
        CancellationToken ct
    ) where T : IProcessManager
    {
        var processManager = await documentSession.LoadAsync<T>(id, ct).ConfigureAwait(false);

        if (processManager is null)
            throw AggregateNotFoundException.For<T>(id);

        handle(processManager);

        documentSession.Update(processManager);

        EnqueueMessages(documentSession, id, processManager);
        await documentSession.SaveChangesAsync(token: ct).ConfigureAwait(false);
    }

    private static void EnqueueMessages<T>(IDocumentSession documentSession, string id, T processManager) where T : IProcessManager
    {
        foreach (var message in processManager.DequeuePendingMessages())
        {
            message.Switch(
                @event => documentSession.Events.Append($"events-{id}", @event),
                command => documentSession.Events.Append($"commands-{id}", command)
            );
        }
    }
}
