using Core.Testing.Fixtures;
using JasperFx.Events.Projections;
using Shouldly;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public record IssueCreated(
        string IssueId,
        string Description
    );

    public record IssueUpdated(
        string IssueId,
        string Description
    );

    namespace OldVersion
    {
        public class Issue
        {
            public string Id { get; set; } = null!;

            public string Description { get; set; } = null!;

            public void Apply(IssueCreated @event)
            {
                Id = @event.IssueId;
                Description = @event.Description;
            }

            public void Apply(IssueUpdated @event)
            {
                Description = @event.Description;
            }
        }
    }

    namespace NewVersion
    {
        public class Issue
        {
            public string Id { get; set; } = null!;

            public string Description { get; set; } = null!;

            public void Apply(IssueCreated @event)
            {
                Id = @event.IssueId;
                Description = $"New Logic: {@event.Description}";
            }

            public void Apply(IssueUpdated @event)
            {
                Description = $"New Logic: {@event.Description}";
            }
        }
    }

    public class Reaggregation(PostgresContainerFixture postgres): MartenTest(postgres.ConnectionString, false)
    {
        public IDocumentSession CreateSessionWithInlineAggregationFor<TIssue>() where TIssue : class, new()
        {
            return base.CreateSession(options =>
            {
                options.Events.AddEventTypes([typeof(IssueCreated), typeof(IssueUpdated)]);
                //It's needed to manualy set that inline aggegation should be applied
                options.Projections.Snapshot<TIssue>(SnapshotLifecycle.Inline);
            });
        }

        [Fact(Skip = "Stopped working in Marten v8")]
        public async Task Given_When_Then()
        {
            var taskId = GenerateRandomId();

            var events = new object[]
            {
                new IssueCreated(taskId, "Issue 1"), new IssueUpdated(taskId, "Issue 1 Updated"),
            };

            OldVersion.Issue issueFromV1InlineAggregation;
            OldVersion.Issue issueFromV1OnlineAggregation;

            await using (var session = CreateSessionWithInlineAggregationFor<OldVersion.Issue>())
            {
                //1. Publish events
                session.Events.StartStream<OldVersion.Issue>(taskId, events);

                await session.SaveChangesAsync(TestContext.Current.CancellationToken);

                issueFromV1InlineAggregation = (await session.LoadAsync<OldVersion.Issue>(taskId, TestContext.Current.CancellationToken))!;
                issueFromV1OnlineAggregation = (await session.Events.AggregateStreamAsync<OldVersion.Issue>(taskId, token: TestContext.Current.CancellationToken))!;
            }

            //2. Both inline and online aggregation for the same type should be the same
            issueFromV1InlineAggregation.Description.ShouldBe("Issue 1 Updated");
            issueFromV1InlineAggregation.Description.ShouldBe(issueFromV1OnlineAggregation.Description);

            //3. Simulate change to aggregation logic
            NewVersion.Issue issueFromV2InlineAggregation;
            NewVersion.Issue issueFromV2OnlineAggregation;

            await using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                issueFromV2InlineAggregation = (await session.LoadAsync<NewVersion.Issue>(taskId, TestContext.Current.CancellationToken))!;
                issueFromV2OnlineAggregation = (await session.Events.AggregateStreamAsync<NewVersion.Issue>(taskId, token: TestContext.Current.CancellationToken))!;
            }

            //4. Inline aggregated snapshot won't change automatically
            issueFromV2InlineAggregation.Description.ShouldBe(issueFromV1InlineAggregation.Description);
            issueFromV2InlineAggregation.Description.ShouldNotBe("New Logic: Issue 1 Updated");

            //5. But online aggregation is being applied automatically
            issueFromV2OnlineAggregation.Description.ShouldNotBe(issueFromV1OnlineAggregation.Description);
            issueFromV2OnlineAggregation.Description.ShouldBe("New Logic: Issue 1 Updated");

            //6. Reagregation
            await using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                //7. Get online aggregation
                //8. Store manually online aggregation as inline aggregation
                session.Store(issueFromV2OnlineAggregation);
                await session.SaveChangesAsync(TestContext.Current.CancellationToken);

                var taskFromV2AfterReaggregation = (await session.LoadAsync<NewVersion.Issue>(taskId, TestContext.Current.CancellationToken))!;

                taskFromV2AfterReaggregation.Description.ShouldNotBe(issueFromV1OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.ShouldBe(issueFromV2OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.ShouldBe("New Logic: Issue 1 Updated");
            }

            await using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                //9. Check if next event would be properly applied to inline aggregation
                session.Events.Append(taskId, new IssueUpdated(taskId, "Completely New text"));
                await session.SaveChangesAsync(TestContext.Current.CancellationToken);
            }

            // TODO: Something has changed here in Marten v7
            // using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            // {
            //     var taskFromV2NewInlineAggregation = await session.LoadAsync<NewVersion.Issue>(taskId)!;
            //     taskFromV2NewInlineAggregation.Description.ShouldBe("New Logic: Completely New text");
            // }
        }
    }
}
