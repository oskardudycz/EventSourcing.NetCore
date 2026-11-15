using Core.Testing.Fixtures;
using Shouldly;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate;

public class EventsAggregation(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString)
{
    public record IssueCreated(
        string IssueId,
        string Description
    );

    public record IssueUpdated(
        string IssueId,
        string Description
    );

    public record IssueRemoved(
        string IssueId
    );

    public record Issue(
        string IssueId,
        string Description
    );

    public class IssuesList
    {
        public string Id { get; set; } = null!;
        public Dictionary<string, Issue> Issues { get; } = new();

        public void Apply(IssueCreated @event)
        {
            var (issueId, description) = @event;
            Issues.Add(issueId, new Issue(issueId, description));
        }

        public void Apply(IssueUpdated @event)
        {
            if (!Issues.ContainsKey(@event.IssueId))
                return;

            Issues[@event.IssueId] = Issues[@event.IssueId]
                with {Description = @event.Description};
        }

        public void Apply(IssueRemoved @event)
        {
            Issues.Remove(@event.IssueId);
        }
    }

    [Fact]
    public async Task GivenStreamOfEvents_WhenAggregateStreamIsCalled_ThenChangesAreAppliedProperly()
    {
        var streamId = GenerateRandomId();

        //1. First Issue Was Created
        var issue1Id = GenerateRandomId();
        EventStore.Append(streamId, new IssueCreated(issue1Id, "Description"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        var issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken);

        issuesList!.Issues.Count().ShouldBe(1);
        issuesList.Issues.Values.Single().IssueId.ShouldBe(issue1Id);
        issuesList.Issues.Values.Single().Description.ShouldBe("Description");

        //2. First Issue Description Was Changed
        EventStore.Append(streamId, new IssueUpdated(issue1Id, "New Description"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken);

        issuesList!.Issues.Count().ShouldBe(1);
        issuesList.Issues.Values.Single().IssueId.ShouldBe(issue1Id);
        issuesList.Issues.Values.Single().Description.ShouldBe("New Description");

        //3. Two Other tasks were added
        EventStore.Append(streamId, new IssueCreated(GenerateRandomId(), "Description2"),
            new IssueCreated(GenerateRandomId(), "Description3"));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken);

        issuesList!.Issues.Count().ShouldBe(3);
        issuesList.Issues.Values.Select(t => t.Description)
            .ShouldBe(["New Description", "Description2", "Description3"], ignoreOrder: true);

        //4. First issue was removed
        EventStore.Append(streamId, new IssueRemoved(issue1Id));
        await Session.SaveChangesAsync(TestContext.Current.CancellationToken);

        issuesList = await EventStore.AggregateStreamAsync<IssuesList>(streamId, token: TestContext.Current.CancellationToken);

        issuesList!.Issues.Count().ShouldBe(2);
        issuesList.Issues.Values.Select(t => t.Description)
            .ShouldBe(["Description2", "Description3"], ignoreOrder: true);
    }
}
