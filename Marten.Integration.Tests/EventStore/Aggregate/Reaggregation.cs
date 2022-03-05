using FluentAssertions;
using Marten.Integration.Tests.TestsInfrastructure;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public record IssueCreated(
        Guid IssueId,
        string Description
    );

    public record IssueUpdated(
        Guid IssueId,
        string Description
    );

    namespace OldVersion
    {
        public class Issue
        {
            public Guid Id { get; set; }

            public string Description { get; set; } = default!;

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
            public Guid Id { get; set; }

            public string Description { get; set; } = default!;

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

    public class Reaggregation: MartenTest
    {
        public Reaggregation() : base(false)
        {
        }

        public IDocumentSession CreateSessionWithInlineAggregationFor<TIssue>() where TIssue : class, new()
        {
            return base.CreateSession(options =>
            {
                options.Events.AddEventTypes(new[] { typeof(IssueCreated), typeof(IssueUpdated) });
                //It's needed to manualy set that inline aggegation should be applied
                options.Projections.SelfAggregate<TIssue>();
            });
        }

        [Fact]
        public void Given_When_Then()
        {
            var taskId = Guid.NewGuid();

            var events = new object[]
            {
                new IssueCreated(taskId, "Issue 1"),
                new IssueUpdated(taskId, "Issue 1 Updated"),
            };

            OldVersion.Issue issueFromV1InlineAggregation;
            OldVersion.Issue issueFromV1OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<OldVersion.Issue>())
            {
                //1. Publish events
                session.Events.StartStream<OldVersion.Issue>(taskId, events);

                session.SaveChanges();

                issueFromV1InlineAggregation = session.Load<OldVersion.Issue>(taskId)!;
                issueFromV1OnlineAggregation = session.Events.AggregateStream<OldVersion.Issue>(taskId)!;
            }

            //2. Both inline and online aggregation for the same type should be the same
            issueFromV1InlineAggregation.Description.Should().Be("Issue 1 Updated");
            issueFromV1InlineAggregation.Description.Should().Be(issueFromV1OnlineAggregation.Description);

            //3. Simulate change to aggregation logic
            NewVersion.Issue issueFromV2InlineAggregation;
            NewVersion.Issue issueFromV2OnlineAggregation;

            using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                issueFromV2InlineAggregation = session.Load<NewVersion.Issue>(taskId)!;
                issueFromV2OnlineAggregation = session.Events.AggregateStream<NewVersion.Issue>(taskId)!;
            }

            //4. Inline aggregated snapshot won't change automatically
            issueFromV2InlineAggregation.Description.Should().Be(issueFromV1InlineAggregation.Description);
            issueFromV2InlineAggregation.Description.Should().NotBe("New Logic: Issue 1 Updated");

            //5. But online aggregation is being applied automatically
            issueFromV2OnlineAggregation.Description.Should().NotBe(issueFromV1OnlineAggregation.Description);
            issueFromV2OnlineAggregation.Description.Should().Be("New Logic: Issue 1 Updated");

            //6. Reagregation
            using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                //7. Get online aggregation
                //8. Store manually online aggregation as inline aggregation
                session.Store(issueFromV2OnlineAggregation);
                session.SaveChanges();

                var taskFromV2AfterReaggregation = session.Load<NewVersion.Issue>(taskId)!;

                taskFromV2AfterReaggregation.Description.Should().NotBe(issueFromV1OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.Should().Be(issueFromV2OnlineAggregation.Description);
                taskFromV2AfterReaggregation.Description.Should().Be("New Logic: Issue 1 Updated");

                //9. Check if next event would be properly applied to inline aggregation
                session.Events.Append(taskId, new IssueUpdated(taskId, "Completely New text"));
                session.SaveChanges();
            }

            using (var session = CreateSessionWithInlineAggregationFor<NewVersion.Issue>())
            {
                var taskFromV2NewInlineAggregation = session.Load<NewVersion.Issue>(taskId)!;
                taskFromV2NewInlineAggregation.Description.Should().Be("New Logic: Completely New text");
            }
        }
    }
}
