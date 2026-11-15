using Core.Testing.Fixtures;
using JasperFx.Events.Projections;
using Shouldly;
using JasperFx;
using JasperFx.Events;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate.OutOfOrder;

public class OutOfOrderProjectionsTest(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
{
    public record IssueCreated(
        string IssueId,
        string Description,
        int IssueVersion
    );

    public record IssueUpdated(
        string IssueId,
        string Description,
        int IssueVersion
    );

    public record Issue(
        string IssueId,
        string Description
    );

    public class IssuesList
    {
        public string Id { get; set; } = null!;
        public Dictionary<string, Issue> Issues { get; set; } = new();

        public void Apply(IssueCreated @event)
        {
            var (issueId, description, _) = @event;
            Issues.Add(issueId, new Issue(issueId, description));
        }

        public void Apply(IssueUpdated @event)
        {
            if (!Issues.ContainsKey(@event.IssueId))
                return;

            Issues[@event.IssueId] = Issues[@event.IssueId]
                with {Description = @event.Description};
        }
    }

    protected override IDocumentSession CreateSession(Action<StoreOptions>? setStoreOptions = null)
    {
        var store = DocumentStore.For(options =>
        {
            options.Connection(ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;

            options.Events.StreamIdentity = StreamIdentity.AsString;

            //It's needed to manually set that inline aggregation should be applied
            options.Projections.Snapshot<IssuesList>(SnapshotLifecycle.Inline);
        });

        return store.LightweightSession();
    }

    [Fact]
    public async Task GivenOutOfOrderEvents_WhenPublishedWithSetVersion_ThenLiveAggregationWorksFine()
    {
        var streamId = GenerateRandomId();
        var firstTaskId = GenerateRandomId();
        var secondTaskId = GenerateRandomId();

        var events = new object[]
        {
            new IssueUpdated(firstTaskId,  "Final First Issue Description", 4),
            new IssueCreated(firstTaskId,  "First Issue", 1),
            new IssueCreated(secondTaskId, "Second Issue 2", 2),
            new IssueUpdated(firstTaskId,  "Intermediate First Issue Description", 3),
            new IssueUpdated(secondTaskId, "Final Second Issue Description", 4),
        };

        //1. Create events
        EventStore.StartStream<IssuesList>(streamId, events);

        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        //2. Get live aggregation
        var issuesListFromLiveAggregation = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken);

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = await Session.LoadAsync<IssuesList>(streamId, TestContext.Current.CancellationToken);

        issuesListFromLiveAggregation.ShouldNotBeNull();
        issuesListFromInlineAggregation.ShouldNotBeNull();

        issuesListFromLiveAggregation!.Issues.Count.ShouldBe(2);
        issuesListFromLiveAggregation.Issues.Count.ShouldBe(issuesListFromInlineAggregation!.Issues.Count);
    }
}
