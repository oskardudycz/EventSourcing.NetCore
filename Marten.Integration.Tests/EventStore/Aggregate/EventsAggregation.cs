using System;
using System.Collections.Generic;
using System.Linq;
using Marten.Integration.Tests.TestsInfrasructure;
using SharpTestsEx;
using Xunit;

namespace Marten.Integration.Tests.EventStore.Aggregate
{
    public class EventsAggregation: MartenTest
    {
        public class IssueCreated
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssueUpdated
        {
            public Guid IssueId { get; set; }
            public string Description { get; set; }
        }

        public class IssueRemoved
        {
            public Guid IssueId { get; set; }
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
                var issue = List.Single(t => t.IssueId == @event.IssueId);

                issue.Description = @event.Description;
            }

            public void Apply(IssueRemoved @event)
            {
                var issue = List.Single(t => t.IssueId == @event.IssueId);

                List.Remove(issue);
            }
        }

        [Fact]
        public void GivenStreamOfEvents_WhenAggregateStreamIsCalled_ThenChangesAreAppliedProperly()
        {
            var streamId = Guid.NewGuid();

            //1. First Issue Was Created
            var issue1Id = Guid.NewGuid();
            EventStore.Append(streamId, new IssueCreated { IssueId = issue1Id, Description = "Description" });
            Session.SaveChanges();

            var issuesList = EventStore.AggregateStream<IssuesList>(streamId);

            issuesList.List.Should().Have.Count.EqualTo(1);
            issuesList.List.Single().IssueId.Should().Be.EqualTo(issue1Id);
            issuesList.List.Single().Description.Should().Be.EqualTo("Description");

            //2. First Issue Description Was Changed
            EventStore.Append(streamId, new IssueUpdated { IssueId = issue1Id, Description = "New Description" });
            Session.SaveChanges();

            issuesList = EventStore.AggregateStream<IssuesList>(streamId);

            issuesList.List.Should().Have.Count.EqualTo(1);
            issuesList.List.Single().IssueId.Should().Be.EqualTo(issue1Id);
            issuesList.List.Single().Description.Should().Be.EqualTo("New Description");

            //3. Two Other tasks were added
            EventStore.Append(streamId, new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description2" },
                new IssueCreated { IssueId = Guid.NewGuid(), Description = "Description3" });
            Session.SaveChanges();

            issuesList = EventStore.AggregateStream<IssuesList>(streamId);

            issuesList.List.Should().Have.Count.EqualTo(3);
            issuesList.List.Select(t => t.Description)
                .Should()
                .Have.SameSequenceAs("New Description", "Description2", "Description3");

            //4. First issue was removed
            EventStore.Append(streamId, new IssueRemoved { IssueId = issue1Id });
            Session.SaveChanges();

            issuesList = EventStore.AggregateStream<IssuesList>(streamId);

            issuesList.List.Should().Have.Count.EqualTo(2);
            issuesList.List.Select(t => t.Description)
                .Should()
                .Have.SameSequenceAs("Description2", "Description3");
        }
    }
}
