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

public class KafkaProducer: IExternalEventProducer
{
    private readonly KafkaProducerConfig config;
    private readonly IActivityScope activityScope1;
    private readonly ILogger<KafkaProducer> logger1;
    private readonly IProducer<string, string> producer;

    public KafkaProducer(IConfiguration configuration,
        IActivityScope activityScope,
        IProducer<string, string>? producer,
        ILogger<KafkaProducer> logger)
    {
        activityScope1 = activityScope;
        logger1 = logger;
        config = configuration.GetKafkaProducerConfig();
        this.producer = producer ?? new ProducerBuilder<string, string>(config.ProducerConfig).Build();
    }

    // get configuration from appsettings.json

    public async Task Publish(IEventEnvelope @event, CancellationToken token)
    {
        try
        {
            await activityScope1.Run($"{nameof(KafkaProducer)}/{nameof(Publish)}",
                async (_, ct) =>
                {
                    using var cts =
                        new CancellationTokenSource(TimeSpan.FromMilliseconds(config.ProducerTimeoutInMs ?? 10000));

                    await producer.ProduceAsync(config.Topic,
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
            logger1.LogError("Error producing Kafka message: {Message} {StackTrace}", e.Message, e.StackTrace);
            throw;
        }
    }
}
