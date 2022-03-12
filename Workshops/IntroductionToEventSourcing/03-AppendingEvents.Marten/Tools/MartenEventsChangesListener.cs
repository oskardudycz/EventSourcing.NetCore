using Marten;
using Marten.Services;

namespace IntroductionToEventSourcing.AppendingEvents.Tools;

public class MartenEventsChangesListener : IDocumentSessionListener
{
    public static readonly MartenEventsChangesListener Instance = new();

    public int AppendedEventsCount { get; private set; }

    public void BeforeSaveChanges(IDocumentSession session)
    {
        AppendedEventsCount = session.PendingChanges.Streams().Sum(s => s.Events.Count);
    }

    public Task BeforeSaveChangesAsync(IDocumentSession session, CancellationToken token)
    {
        AppendedEventsCount = session.PendingChanges.Streams().Sum(s => s.Events.Count);
        return Task.CompletedTask;
    }

    public void AfterCommit(IDocumentSession session, IChangeSet commit)
    {

    }

    public Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
    {
        return Task.CompletedTask;
    }

    public void DocumentLoaded(object id, object document)
    {

    }

    public void DocumentAddedForStorage(object id, object document)
    {

    }
}
