using Marten;

namespace ProjectManagement.Core.Marten;

public static class CommandHandling
{
    public static Task ComposeAsync(this IDocumentSession documentSession, params (Guid, object)[] events)
    {
        foreach (var (streamId, @event) in events)
        {
            documentSession.Events.Append(streamId, @event);
        }

        return documentSession.SaveChangesAsync();
    }
}
