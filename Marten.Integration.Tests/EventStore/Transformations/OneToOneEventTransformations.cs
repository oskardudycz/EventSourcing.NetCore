// using System;
// using System.Linq;
// using Marten.Events;
// using Marten.Events.Projections;
// using Marten.Integration.Tests.TestsInfrasructure;
// using SharpTestsEx;
// using Xunit;
//
// namespace Marten.Integration.Tests.EventStore.Transformations
// {
//     public class OneToOneEventTransformations: MartenTest
//     {
//         public interface IIssueEvent
//         {
//             Guid IssueId { get; set; }
//         }
//
//         public class IssueCreated: IIssueEvent
//         {
//             public Guid IssueId { get; set; }
//             public string Description { get; set; }
//         }
//
//         public class IssueUpdated: IIssueEvent
//         {
//             public Guid IssueId { get; set; }
//             public string Description { get; set; }
//         }
//
//         public enum ChangeType
//         {
//             Creation,
//             Modification
//         }
//
//         public class IssueChangesLog
//         {
//             public Guid Id { get; set; }
//             public Guid IssueId { get; set; }
//             public ChangeType ChangeType { get; set; }
//             public DateTime Timestamp { get; set; }
//         }
//
//         public class IssuesList { }
//
//         public class IssueChangeLogTransform: ITransform<IssueCreated, IssueChangesLog>,
//             ITransform<IssueUpdated, IssueChangesLog>
//         {
//             public IssueChangesLog Transform(EventStream stream, Event<IssueCreated> input)
//             {
//                 return new IssueChangesLog
//                 {
//                     IssueId = input.Data.IssueId,
//                     Timestamp = input.Timestamp.DateTime,
//                     ChangeType = ChangeType.Creation
//                 };
//             }
//
//             public IssueChangesLog Transform(EventStream stream, Event<IssueUpdated> input)
//             {
//                 return new IssueChangesLog
//                 {
//                     IssueId = input.Data.IssueId,
//                     Timestamp = input.Timestamp.DateTime,
//                     ChangeType = ChangeType.Modification
//                 };
//             }
//         }
//
//         protected override IDocumentSession CreateSession(Action<StoreOptions> setStoreOptions)
//         {
//             var store = DocumentStore.For(options =>
//             {
//                 options.Connection(Settings.ConnectionString);
//                 options.AutoCreateSchemaObjects = AutoCreate.All;
//                 options.DatabaseSchemaName = SchemaName;
//                 options.Events.DatabaseSchemaName = SchemaName;
//
//                 //It's needed to manualy set that transformations should be applied
//                 options.Events.InlineProjections.TransformEvents<IssueCreated, IssueChangesLog>(new IssueChangeLogTransform());
//                 options.Events.InlineProjections.TransformEvents<IssueUpdated, IssueChangesLog>(new IssueChangeLogTransform());
//             });
//
//             return store.OpenSession();
//         }
//
//         [Fact]
//         public void GivenEvents_WhenInlineTransformationIsApplied_ThenReturnsSameNumberOfTransformedItems()
//         {
//             var issue1Id = Guid.NewGuid();
//             var issue2Id = Guid.NewGuid();
//
//             var events = new IIssueEvent[]
//             {
//                 new IssueCreated {IssueId = issue1Id, Description = "Description 1"},
//                 new IssueUpdated {IssueId = issue1Id, Description = "Description 1 New"},
//                 new IssueCreated {IssueId = issue2Id, Description = "Description 2"},
//                 new IssueUpdated {IssueId = issue1Id, Description = "Description 1 Super New"},
//                 new IssueUpdated {IssueId = issue2Id, Description = "Description 2 New"},
//             };
//
//             //1. Create events
//             EventStore.StartStream<IssuesList>(events);
//
//             Session.SaveChanges();
//
//             //2. Get transformed events
//             var changeLogs = Session.Query<IssueChangesLog>().ToList();
//
//             changeLogs.Should().HaveCount(events.Length);
//
//             changeLogs.Select(ev => ev.IssueId)
//                 .Should().Have.SameValuesAs(events.Select(ev => ev.IssueId));
//
//             changeLogs.Count(ev => ev.ChangeType == ChangeType.Creation)
//                 .Should().Be(events.OfType<IssueCreated>().Count());
//
//             changeLogs.Count(ev => ev.ChangeType == ChangeType.Modification)
//                 .Should().Be(events.OfType<IssueUpdated>().Count());
//
//             changeLogs.Count(ev => ev.IssueId == issue1Id)
//                 .Should().Be(events.Count(ev => ev.IssueId == issue1Id));
//
//             changeLogs.Count(ev => ev.IssueId == issue2Id)
//                 .Should().Be(events.Count(ev => ev.IssueId == issue2Id));
//         }
//     }
// }
