using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Serialization.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Kafka.Producers;

public class KafkaProducer: IExternalEventProducer
{
    private readonly ILogger<KafkaProducer> logger;
    private readonly KafkaProducerConfig config;

    public KafkaProducer(
        IConfiguration configuration,
        ILogger<KafkaProducer> logger
    )
    {
        this.logger = logger;
        // get configuration from appsettings.json
        config = configuration.GetKafkaProducerConfig();
    }

    public async Task Publish(IEventEnvelope @event, CancellationToken ct)
    {
        try
        {
            using var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build();
            // publish event to kafka topic taken from config

            await p.ProduceAsync(config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.Data.GetType().Name,
                    // serialize event to message Value
                    Value = @event.ToJson()
                }, ct).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}",e.Message, e.StackTrace);
            throw;
        }
    }
}
