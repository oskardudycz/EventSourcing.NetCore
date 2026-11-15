using Core.Testing.Fixtures;
using JasperFx.Events.Projections;
using Shouldly;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate;

public class InlineAggregationStorage(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
{
    public record IssueCreated(
        string IssueId,
        string Description
    );

    public record IssueUpdated(
        string IssueId,
        string Description
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
            var (issueId, description) = @event;
            Issues.Add(issueId, new Issue(issueId, description));
        }

        public void Apply(IssueUpdated @event)
        {
            Issues[@event.IssueId] = Issues[@event.IssueId]
                with
                {
                    Description = @event.Description
                };
        }
    }

    protected override IDocumentSession CreateSession(Action<StoreOptions>? storeOptions = null) =>
        base.CreateSession(options =>
        {
            //It's needed to manually set that inline aggregation should be applied
            options.Projections.Snapshot<IssuesList>(SnapshotLifecycle.Inline);
        });

    [Fact]
    public async Task GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var streamId = GenerateRandomId();
        var issue1Id = GenerateRandomId();
        var issue2Id = GenerateRandomId();

        var events = new object[]
        {
            new IssueCreated(issue1Id, "Description 1"), new IssueUpdated(issue1Id, "Description 1 New"),
            new IssueCreated(issue2Id, "Description 2"), new IssueUpdated(issue1Id, "Description 1 Super New"),
            new IssueUpdated(issue2Id, "Description 2 New"),
        };

        //1. Create events
        EventStore.StartStream<IssuesList>(streamId, events);

        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        //2. Get live aggregation
        var issuesListFromLiveAggregation = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken)!;

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = await Session.LoadAsync<IssuesList>(streamId, TestContext.Current.CancellationToken);

        issuesListFromLiveAggregation.ShouldNotBeNull();
        issuesListFromInlineAggregation.ShouldNotBeNull();

        issuesListFromLiveAggregation!.Issues.Count.ShouldBe(2);
        issuesListFromLiveAggregation.Issues.Count.ShouldBe(issuesListFromInlineAggregation!.Issues.Count);
    }
}
