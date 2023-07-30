using System.Text.Json;
using Confluent.Kafka;
using Marten;
using Marten.Events;
using Marten.Events.Projections;

namespace Helpdesk.Api.Core.Kafka;

public class KafkaProducer: IProjection
{
    private const string DefaultConfigKey = "KafkaProducer";

    private readonly KafkaProducerConfig config;

    public KafkaProducer(IConfiguration configuration)
    {
        config = configuration.GetRequiredSection(DefaultConfigKey).Get<KafkaProducerConfig>() ??
                 throw new InvalidOperationException();
    }

    public async Task ApplyAsync(IDocumentOperations operations, IReadOnlyList<StreamAction> streamsActions,
        CancellationToken ct)
    {
        foreach (var @event in streamsActions.SelectMany(streamAction => streamAction.Events))
        {
            await Publish(@event.Data, ct);
        }
    }

    public void Apply(IDocumentOperations operations, IReadOnlyList<StreamAction> streams) =>
        throw new NotImplementedException("Producer should be only used in the AsyncDaemon");

    private async Task Publish(object @event, CancellationToken ct)
    {
        try
        {
            using var producer = new ProducerBuilder<string, string>(config.ProducerConfig).Build();

            await producer.ProduceAsync(config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.GetType().Name,
                    // serialize event to message Value
                    Value = JsonSerializer.Serialize(@event)
                }, ct).ConfigureAwait(false);
        }
        catch (Exception exc)
        {
            Console.WriteLine(exc.Message);
            throw;
        }
    }
}

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string? Topic { get; set; }
}
