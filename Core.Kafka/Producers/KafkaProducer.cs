using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Core.Kafka.Producers;

public class KafkaProducer: IExternalEventProducer
{
    private readonly KafkaProducerConfig config;

    public KafkaProducer(
        IConfiguration configuration
    )
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // get configuration from appsettings.json
        config = configuration.GetKafkaProducerConfig();
    }

    public async Task Publish(IExternalEvent @event, CancellationToken cancellationToken)
    {
        using var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build();
        await Task.Yield();
        // publish event to kafka topic taken from config
        await p.ProduceAsync(config.Topic,
            new Message<string, string>
            {
                // store event type name in message Key
                Key = @event.GetType().Name,
                // serialize event to message Value
                Value = JsonConvert.SerializeObject(@event)
            }, cancellationToken);
    }
}
