using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Weasel.Core;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate.OutOfOrder;

public class OutOfOrderProjectionsTest: MartenTest
{
    public record IssueCreated(
        Guid IssueId,
        string Description,
        int IssueVersion
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description,
        int IssueVersion
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
            options.Connection(Settings.ConnectionString);
            options.AutoCreateSchemaObjects = AutoCreate.All;
            options.DatabaseSchemaName = SchemaName;
            options.Events.DatabaseSchemaName = SchemaName;

            //It's needed to manually set that inline aggregation should be applied
            options.Projections.SelfAggregate<IssuesList>();
        });

        return store.OpenSession();
    }

    [Fact]
    public void GivenOutOfOrderEvents_WhenPublishedWithSetVersion_ThenLiveAggregationWorksFine()
    {
        var firstTaskId = Guid.NewGuid();
        var secondTaskId = Guid.NewGuid();

        var events = new object[]
        {
            new IssueUpdated(firstTaskId,  "Final First Issue Description", 4),
            new IssueCreated(firstTaskId,  "First Issue", 1),
            new IssueCreated(secondTaskId, "Second Issue 2", 2),
            new IssueUpdated(firstTaskId,  "Intermediate First Issue Description", 3),
            new IssueUpdated(secondTaskId, "Final Second Issue Description", 4),
        };

        //1. Create events
        var streamId = EventStore.StartStream<IssuesList>(events).Id;

        Session.SaveChanges();

        //2. Get live agregation
        var issuesListFromLiveAggregation = EventStore.AggregateStream<IssuesList>(streamId);

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = Session.Load<IssuesList>(streamId);

        issuesListFromLiveAggregation.Should().NotBeNull();
        issuesListFromInlineAggregation.Should().NotBeNull();

        issuesListFromLiveAggregation!.Issues.Count.Should().Be(2);
        issuesListFromLiveAggregation!.Issues.Count.Should().Be(issuesListFromInlineAggregation!.Issues.Count);
    }
}
