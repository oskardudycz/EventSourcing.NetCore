using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate;

public class InlineAggregationStorage: MartenTest
{
    public record IssueCreated(
        Guid IssueId,
        string Description
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description
    );

    public record Issue(
        Guid IssueId,
        string Description
    );

    public class IssuesList
    {
        public Guid Id { get; set; }
        public Dictionary<Guid, Issue> Issues { get; } = new();

        public void Apply(IssueCreated @event)
        {
            var (issueId, description) = @event;
            Issues.Add(issueId, new Issue(issueId, description));
        }

        public void Apply(IssueUpdated @event)
        {
            Issues[@event.IssueId] = Issues[@event.IssueId]
                with {Description = @event.Description};
        }
    }

    protected override IDocumentSession CreateSession(Action<StoreOptions>? storeOptions = null)
    {
        return base.CreateSession(options =>
        {
            //It's needed to manually set that inline aggregation should be applied
            options.Projections.SelfAggregate<IssuesList>();
        });
    }

    [Fact]
    public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var issue1Id = Guid.NewGuid();
        var issue2Id = Guid.NewGuid();

        var events = new object[]
        {
            new IssueCreated(issue1Id, "Description 1"),
            new IssueUpdated(issue1Id, "Description 1 New"),
            new IssueCreated(issue2Id, "Description 2"),
            new IssueUpdated(issue1Id, "Description 1 Super New"),
            new IssueUpdated(issue2Id, "Description 2 New"),
        };

        //1. Create events
        var streamId = EventStore.StartStream<IssuesList>(events).Id;

        Session.SaveChanges();

        //2. Get live aggregation
        var issuesListFromLiveAggregation = EventStore.AggregateStream<IssuesList>(streamId)!;

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = Session.Load<IssuesList>(streamId)!;

        issuesListFromLiveAggregation.Should().NotBeNull();
        issuesListFromInlineAggregation.Should().NotBeNull();

        issuesListFromLiveAggregation.Issues.Count.Should().Be(2);
        issuesListFromLiveAggregation.Issues.Count.Should().Be(issuesListFromInlineAggregation.Issues.Count);
    }
}
