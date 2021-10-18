using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Baseline.Dates;
using Core.Threading;
using FluentAssertions;
using Marten.Events;
using Marten.Events.Projections;
using Marten.Integration.Tests.TestsInfrastructure;
using Weasel.Postgresql;
using Xunit;

namespace Marten.Integration.Tests.Integration
{
    public record UserAdded(Guid UserId);

    public record CompanyAdded(Guid CompanyId);

    public static class Storage
    {
    }

    public interface IMartenEventsConsumer
    {
        Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions);
    }

    public class MartenEventsConsumer: IMartenEventsConsumer
    {
        public static List<object> Events { get; } = new();

        public Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
        {
            foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
            {
                Events.Add(@event);
                Console.WriteLine($"{@event.Sequence} - {@event.EventTypeName}");
            }
            return Task.CompletedTask;
        }
    }

    public class MartenSubscription: IProjection
    {
        private readonly IMartenEventsConsumer consumer;

        public MartenSubscription(IMartenEventsConsumer consumer)
        {
            this.consumer = consumer;
        }

        public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams)
        {
            using (NoSynchronizationContextScope.Enter())
            {
                consumer.ConsumeAsync(streams).Wait();
            }
        }

        public Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streams, CancellationToken cancellation)
        {
            return consumer.ConsumeAsync(streams);
        }
    }

    public class Subscriptions: MartenTest
    {
        [Fact]
        public async Task AsyncDaemon_Should_PublishEvents_ToMartenSubscription()
        {
            using var daemon = Session.DocumentStore.BuildProjectionDaemon();
            await daemon.StartDaemon();
            await daemon.StartAllShards();

            for(var i = 0; i < 10; i++ )
            {
                var userId = Guid.NewGuid();
                Session.Events.Append(userId, new UserAdded(userId));
                var companyId= Guid.NewGuid();
                Session.Events.Append(companyId, new CompanyAdded(companyId));

                await Session.SaveChangesAsync();
            }

            await daemon.Tracker.WaitForHighWaterMark(20, 30.Seconds());
            await daemon.Tracker.WaitForShardState("customConsumer:All", 20, 30.Seconds());

            daemon.Tracker.HighWaterMark.Should().Be(20);
            MartenEventsConsumer.Events.Should().HaveCount(20);

            await daemon.StopAll();
        }

        protected override IDocumentSession CreateSession(Action<StoreOptions>? setStoreOptions = null)
        {
            var store = DocumentStore.For(options =>
            {
                options.Connection(Settings.ConnectionString);
                options.AutoCreateSchemaObjects = AutoCreate.All;
                options.DatabaseSchemaName = SchemaName;
                options.Events.DatabaseSchemaName = SchemaName;

                options.Projections.Add(new MartenSubscription(
                    new MartenEventsConsumer()),
                    ProjectionLifecycle.Async,
                    "customConsumer");
            });

            return store.OpenSession();
        }
    }
}
