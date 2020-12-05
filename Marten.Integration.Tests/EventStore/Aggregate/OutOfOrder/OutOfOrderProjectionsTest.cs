using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Projections
{
    public class OutOfOrderProjectionsTest: MartenTest
    {
        public interface IIssueEvent
        {
            Guid IssueId { get; set; }

            int IssueVersion { get; set; }
        }

        public class IssueCreated: IIssueEvent
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }

            public int IssueVersion { get; set; }
        }

        public class IssueUpdated: IIssueEvent
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }

            public int IssueVersion { get; set; }
        }

        public class Issue
        {
            public Guid IssueId { get; set; }

            public string Description { get; set; }
        }

        public class IssuesList
        {
            public Guid Id { get; set; }
            public List<Issue> List { get; private set; }

            public IssuesList()
            {
                List = new List<Issue>();
            }

            public void Apply(IssueCreated @event)
            {
                List.Add(new Issue { IssueId = @event.IssueId, Description = @event.Description });
            }

            public void Apply(IssueUpdated @event)
            {
                var issue = List.SingleOrDefault(t => t.IssueId == @event.IssueId);

                if (issue == null)
                {
                    return;
                }

                issue.Description = @event.Description;
            }
        }

        protected override IDocumentSession CreateSession(Action<StoreOptions> setStoreOptions)
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(Settings.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = SchemaName;
                options.Events.DatabaseSchemaName = SchemaName;

                //It's needed to manualy set that inline aggegation should be applied
                options.Events.InlineProjections.AggregateStreamsWith<IssuesList>();
            });

            return store.OpenSession();
        }

        [Fact]
        public void GivenOutOfOrderEvents_WhenPublishedWithSetVersion_ThenLiveAggregationWorksFine()
        {
            var firstTaskId = Guid.NewGuid();
            var secondTaskId = Guid.NewGuid();

            var events = new IIssueEvent[]
            {
                new IssueUpdated {IssueId = firstTaskId, Description = "Final First Issue Description", IssueVersion = 4 },
                new IssueCreated {IssueId = firstTaskId, Description = "First Issue", IssueVersion = 1 },
                new IssueCreated {IssueId = secondTaskId, Description = "Second Issue 2", IssueVersion = 2 },
                new IssueUpdated {IssueId = firstTaskId, Description = "Intermediate First Issue Description", IssueVersion = 3},
                new IssueUpdated {IssueId = secondTaskId, Description = "Final Second Issue Description", IssueVersion = 5},
            };

            //1. Create events
            var streamId = EventStore.StartStream<IssuesList>(events).Id;

            Session.SaveChanges();

            //2. Get live agregation
            var issuesListFromLiveAggregation = EventStore.AggregateStream<IssuesList>(streamId);

            //3. Get inline aggregation
            var issuesListFromInlineAggregation = Session.Load<IssuesList>(streamId);

            issuesListFromLiveAggregation.Should().Not.Be.Null();
            issuesListFromInlineAggregation.Should().Not.Be.Null();

            issuesListFromLiveAggregation.List.Count.Should().Be.EqualTo(2);
            issuesListFromLiveAggregation.List.Count.Should().Be.EqualTo(issuesListFromInlineAggregation.List.Count);
        }
    }
}
