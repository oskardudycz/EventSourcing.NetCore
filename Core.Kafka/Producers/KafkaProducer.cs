using System.Diagnostics;
using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.OpenTelemetry;
using Core.OpenTelemetry.Serialization;
using Core.Serialization.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Kafka.Producers;

public class KafkaProducer: IExternalEventProducer
{
    private readonly IActivityScope activityScope;
    private readonly ILogger<KafkaProducer> logger;
    private readonly KafkaProducerConfig config;

    public KafkaProducer(
        IConfiguration configuration,
        IActivityScope activityScope,
        ILogger<KafkaProducer> logger
    )
    {
        this.activityScope = activityScope;
        this.logger = logger;
        // get configuration from appsettings.json
        config = configuration.GetKafkaProducerConfig();
    }

    public async Task Publish(IEventEnvelope @event, CancellationToken token)
    {
        try
        {
            await activityScope.Run($"{nameof(KafkaProducer)}/{nameof(Publish)}",
                async (_, ct) =>
                {
                    using var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build();
                    // publish event to kafka topic taken from config

                    await p.ProduceAsync(config.Topic,
                        new Message<string, string>
                        {
                            // store event type name in message Key
                            Key = @event.Data.GetType().Name,
                            // serialize event to message Value
                            Value = @event.ToJson(new PropagationContextJsonConverter())
                        }, ct).ConfigureAwait(false);
                },
                new StartActivityOptions
                {
                    Tags = { { TelemetryTags.EventHandling.Event, @event.Data.GetType() } },
                    Kind = ActivityKind.Producer
                },
                token
            );
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}",e.Message, e.StackTrace);
            throw;
        }
    }
}
