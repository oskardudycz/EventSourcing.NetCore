using System.Text.Json;
using Confluent.Kafka;
using Marten;
using JasperFx.Events;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten.Events.Daemon;
using Marten.Events.Daemon.Internals;
using Marten.Subscriptions;

namespace Helpdesk.Api.Core.Kafka;

public class KafkaProducer(IConfiguration configuration): SubscriptionBase
{
    private const string DefaultConfigKey = "KafkaProducer";

    private readonly KafkaProducerConfig config =
        configuration.GetRequiredSection(DefaultConfigKey).Get<KafkaProducerConfig>() ??
        throw new InvalidOperationException();

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange eventRange,
        ISubscriptionController subscriptionController,
        IDocumentOperations operations,
        CancellationToken ct
    )
    {
        foreach (var @event in eventRange.Events)
        {
            await Publish(subscriptionController, @event, ct);
        }
        return NullChangeListener.Instance;
    }

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams) =>
        throw new NotImplementedException("Producer should be only used in the AsyncDaemon");

    private async Task Publish(ISubscriptionController subscriptionController, IEvent @event, CancellationToken ct)
    {
        try
        {
            using var producer = new ProducerBuilder<string, string>(config.ProducerConfig).Build();

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(config.ProducerTimeoutInMs ?? 1000));

            await producer.ProduceAsync(config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.GetType().Name,
                    // serialize event to message Value
                    Value = JsonSerializer.Serialize(@event.Data)
                }, cts.Token).ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            await subscriptionController.ReportCriticalFailureAsync(exc, @event.Sequence);
            // TODO: you can also differentiate based on the exception
            // await subscriptionController.RecordDeadLetterEventAsync(@event, exc);
            Console.WriteLine(exc.Message);
            throw;
        }
    }
}

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string? Topic { get; set; }

    public int? ProducerTimeoutInMs { get; set; }
}
