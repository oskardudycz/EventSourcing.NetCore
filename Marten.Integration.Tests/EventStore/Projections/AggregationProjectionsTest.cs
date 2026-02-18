using FluentAssertions;
using JasperFx;
using JasperFx.Events.Projections;
using Marten.Events.Aggregation;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections;

public class AggregationProjectionsTest(MartenFixture fixture): MartenTest(fixture.PostgreSqlContainer)
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
                with
                {
                    Description = @event.Description
                };
        }
    }

    public class IssueDescriptions
    {
        public Guid Id { get; set; }
        public Dictionary<Guid, string> Descriptions { get; } = new();

        public void Apply(IssueCreated @event)
        {
            Descriptions.Add(@event.IssueId, @event.Description);
        }

        public void Apply(IssueUpdated @event)
        {
            Descriptions[@event.IssueId] = @event.Description;
        }
    }

    public class IssueDescriptionsProjection: SingleStreamProjection<IssueDescriptions, string>
    {
        public void Apply(IssueCreated @event, IssueDescriptions item)
        {
            item.Apply(@event);
        }

        public void Apply(IssueUpdated @event, IssueDescriptions item)
        {
            item.Apply(@event);
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

            //It's needed to manually set that inline aggregation should be applied
            options.Projections.Snapshot<IssuesList>(SnapshotLifecycle.Inline);
            options.Projections.Add(new IssueDescriptionsProjection(), ProjectionLifecycle.Inline);
        });

        return store.LightweightSession();
    }

    [Fact]
    public async Task GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
    {
        var issue1Id = Guid.NewGuid();
        var issue2Id = Guid.NewGuid();

        var events = new object[]
        {
            new IssueCreated(issue1Id, "Description 1"), new IssueUpdated(issue1Id, "Description 1 New"),
            new IssueCreated(issue2Id, "Description 2"), new IssueUpdated(issue1Id, "Description 1 Super New"),
            new IssueUpdated(issue2Id, "Description 2 New"),
        };

        //1. Create events
        var streamId = EventStore.StartStream<IssuesList>(events).Id;

        await Session.SaveChangesAsync();

        //2. Get live agregation
        var issuesListFromLiveAggregation = await EventStore.AggregateStreamAsync<IssuesList>(streamId);

        //3. Get inline aggregation
        var issuesListFromInlineAggregation = await Session.LoadAsync<IssuesList>(streamId);

        var projection = Session.Query<IssueDescriptions>().FirstOrDefault();

        issuesListFromLiveAggregation.Should().NotBeNull();
        issuesListFromInlineAggregation.Should().NotBeNull();
        projection.Should().NotBeNull();

        issuesListFromLiveAggregation!.Issues.Count.Should().Be(2);
        issuesListFromInlineAggregation!.Issues.Count.Should().Be(2);
        projection!.Descriptions.Count.Should().Be(2);
    }
}
