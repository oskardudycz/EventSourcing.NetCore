using System.Diagnostics;
using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.OpenTelemetry;
using Core.OpenTelemetry.Serialization;
using Core.Serialization.Newtonsoft;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Core.Extensions.DictionaryExtensions;

namespace Core.Kafka.Producers;

public class KafkaProducer(
    IConfiguration configuration,
    IActivityScope activityScope,
    ILogger<KafkaProducer> logger
): IExternalEventProducer
{
    private readonly KafkaProducerConfig config = configuration.GetKafkaProducerConfig();

    // get configuration from appsettings.json

    public async Task Publish(IEventEnvelope @event, CancellationToken token)
    {
        try
        {
            await activityScope.Run($"{nameof(KafkaProducer)}/{nameof(Publish)}",
                async (_, ct) =>
                {
                    using var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build();
                    // publish event to kafka topic taken from config

                    using var cts =
                        new CancellationTokenSource(TimeSpan.FromMilliseconds(config.ProducerTimeoutInMs ?? 1000));

                    await p.ProduceAsync(config.Topic,
                        new Message<string, string>
                        {
                            // store event type name in message Key
                            Key = @event.Data.GetType().Name,
                            // serialize event to message Value
                            Value = @event.ToJson(new PropagationContextJsonConverter())
                        }, cts.Token).ConfigureAwait(false);
                },
                new StartActivityOptions
                {
                    Tags = Merge(
                        TelemetryTags.Messaging.Kafka.ProducerTags(
                            config.Topic,
                            config.Topic,
                            @event.Data.GetType().Name
                        ),
                        new Dictionary<string, object?>
                        {
                            { TelemetryTags.EventHandling.Event, @event.Data.GetType() }
                        }),
                    Kind = ActivityKind.Producer
                },
                token
            ).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }
}
