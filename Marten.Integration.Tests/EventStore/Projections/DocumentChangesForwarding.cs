using Core.Testing;
using Marten.Events.Aggregation;
using Marten.Events.Daemon.Resiliency;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Marten.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections;

public record ProjectCreated(
    Guid ProjectId,
    string Name
);

public record ProjectStarted(
    Guid ProjectId,
    DateTimeOffset StartedAt
);

public record ManagerAssignedToProject(
    Guid ProjectId,
    Guid ManagerId
);

public record ProjectInfo(
    Guid Id,
    string Name,
    DateTimeOffset? StartedAt = null,
    Guid? ManagerId = null
);

public class ProjectInfoProjection: SingleStreamProjection<ProjectInfo>
{
    public static ProjectInfo Create(ProjectCreated created) =>
        new(created.ProjectId, created.Name);

    public ProjectInfo Apply(ProjectStarted started, ProjectInfo current) =>
        current with { StartedAt = started.StartedAt };

    public ProjectInfo Apply(ManagerAssignedToProject managerAssigned, ProjectInfo current) =>
        current with { ManagerId = managerAssigned.ManagerId };
}

public interface IMessagingSystem
{
    Task Publish(object[] messages, CancellationToken ct);
}

public class MessagingSystemStub: IMessagingSystem
{
    private bool shouldFail;

    public Task Publish(object[] messages, CancellationToken ct)
    {
        shouldFail = !shouldFail;

        return shouldFail ? Task.FromException(new InvalidOperationException()) : Task.CompletedTask;
    }
}

public enum ChangeType
{
    Insert,
    Update,
    Delete
}

public record DocumentChanged(ChangeType ChangeType, object Data);

public class AsyncListenerWrapper(EventListener eventListener, IChangeListener inner): IChangeListener
{
    public async Task BeforeCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
    {
        await inner.BeforeCommitAsync(session, commit, token);

        foreach (var @event in commit.Inserted.Select(doc => new DocumentChanged(ChangeType.Insert, doc))
                     .Union(commit.Updated.Select(doc => new DocumentChanged(ChangeType.Update, doc)))
                     .Union(commit.Deleted.Select(doc => new DocumentChanged(ChangeType.Delete, doc))))
        {
            await eventListener.Handle(@event, token);
        }
    }

    public Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token) =>
        Task.CompletedTask;
}

public class AsyncDocumentChangesForwarder(IMessagingSystem messagingSystem): IChangeListener
{
    public Task BeforeCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token)
    {
        var changes = commit.Inserted.Select(doc => new DocumentChanged(ChangeType.Insert, doc))
            .Union(commit.Updated.Select(doc => new DocumentChanged(ChangeType.Update, doc)))
            .Union(commit.Deleted.Select(doc => new DocumentChanged(ChangeType.Delete, doc)))
            .ToArray();

        return messagingSystem.Publish(changes.Cast<object>().ToArray(), token);
    }

    public Task AfterCommitAsync(IDocumentSession session, IChangeSet commit, CancellationToken token) =>
        Task.CompletedTask;
}

public class DocumentChangesForwarding(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer, false, false)
{
    [Fact(Skip = "Some weird is happening in System channels")]
    public async Task GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var cts = new CancellationTokenSource();
        cts.CancelAfter(10000);

        await daemon.StartAsync(cts.Token);

        var projectId = Guid.NewGuid();
        var name = "Test";

        var projectCreated = new ProjectCreated(projectId, name);

        documentSession.Events.StartStream(projectId, projectCreated);
        await documentSession.SaveChangesAsync(cts.Token);

        await eventListener.WaitForProcessing(new DocumentChanged(ChangeType.Update, new ProjectInfo(projectId, name)),
            cts.Token);
    }

    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();

        var services = new ServiceCollection();

        services.AddLogging();

        services.AddMarten(options =>
            {
                options.DatabaseSchemaName = SchemaName;
                options.Connection(ConnectionString);
                options.Projections.Add<ProjectInfoProjection>(ProjectionLifecycle.Async);
                options.Projections.AsyncListeners.Add(
                    new AsyncListenerWrapper(
                        eventListener,
                        new AsyncDocumentChangesForwarder(messagingSystemStub)
                    )
                );
            }).AddAsyncDaemon(DaemonMode.Solo)
            .UseLightweightSessions();

        await using var sp = services.BuildServiceProvider();
        serviceScope = sp.CreateScope();
        daemon = serviceScope.ServiceProvider.GetRequiredService<IHostedService>();
        documentSession = serviceScope.ServiceProvider.GetRequiredService<IDocumentSession>();
    }

    private IHostedService daemon = default!;
    private IServiceScope serviceScope = default!;
    private IDocumentSession documentSession = default!;
    private EventListener eventListener = new();
    private MessagingSystemStub messagingSystemStub = new();

    public override async Task DisposeAsync()
    {
        await daemon.StopAsync(default);
        serviceScope.Dispose();

        await base.DisposeAsync();
    }
}
