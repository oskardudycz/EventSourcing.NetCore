using System.Diagnostics;
using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Kafka.Events;
using Core.Kafka.Producers;
using Core.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using static Core.Extensions.DictionaryExtensions;

namespace Core.Kafka.Consumers;

public class KafkaConsumer: IExternalEventConsumer
{
    private readonly KafkaConsumerConfig config;
    private readonly IEventBus eventBus;
    private readonly IActivityScope activityScope;
    private readonly ILogger<KafkaConsumer> logger;

    public KafkaConsumer(
        IConfiguration configuration,
        IEventBus eventBus,
        IActivityScope activityScope,
        ILogger<KafkaConsumer> logger
    )
    {

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        this.eventBus = eventBus;
        this.activityScope = activityScope;
        this.logger = logger;

        // get configuration from appsettings.json
        config = configuration.GetKafkaConsumerConfig();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka consumer started");

        // create consumer
        using var consumer = new ConsumerBuilder<string, string>(config.ConsumerConfig).Build();
        // subscribe to Kafka topics (taken from config)
        consumer.Subscribe(config.Topics);
        try
        {
            // keep consumer working until it get signal that it should be shuted down
            while (!cancellationToken.IsCancellationRequested)
            {
                // consume event from Kafka
                await ConsumeNextEvent(consumer, cancellationToken);
            }
        }
        catch (Exception e)
        {
            logger.LogError("Error consuming Kafka message: {Message} {StackTrace}",e.Message, e.StackTrace);

            // Ensure the consumer leaves the group cleanly and final offsets are committed.
            consumer.Close();
        }
    }

    private async Task ConsumeNextEvent(IConsumer<string, string> consumer, CancellationToken token)
    {
        try
        {
            //lol ^_^ - remove this hack when this GH issue is solved: https://github.com/dotnet/extensions/issues/2149#issuecomment-518709751
            await Task.Yield();
            // wait for the upcoming message, consume it when arrives
            var message = consumer.Consume(token);

            // get event type from name stored in message.Key
            var eventEnvelope = message.ToEventEnvelope();

            if (eventEnvelope == null)
            {
                // That can happen if we're sharing database between modules.
                // If we're subscribing to all and not filtering out events from other modules,
                // then we might get events that are from other module and we might not be able to deserialize them.
                // In that case it's safe to ignore deserialization error.
                // You may add more sophisticated logic checking if it should be ignored or not.
                logger.LogWarning("Couldn't deserialize event of type: {EventType}", message.Message.Key);

                if (!config.IgnoreDeserializationErrors)
                    throw new InvalidOperationException(
                        $"Unable to deserialize event {message.Message.Key}"
                    );

                return;
            }

            await activityScope.Run($"{nameof(KafkaConsumer)}/{nameof(ConsumeNextEvent)}",
                async (_, ct) =>
                {
                    // publish event to internal event bus
                    await eventBus.Publish(eventEnvelope, ct);

                    consumer.Commit();
                },
                new StartActivityOptions
                {
                    Tags = Merge(
                        TelemetryTags.Messaging.Kafka.ConsumerTags(
                            config.ConsumerConfig.GroupId,
                            message.Topic,
                            message.Message.Key,
                            message.Partition.Value.ToString(),
                            config.ConsumerConfig.GroupId
                        ),
                        new Dictionary<string, object?>
                        {
                            { TelemetryTags.EventHandling.Event, eventEnvelope.Data.GetType() }
                        }),
                    Parent = eventEnvelope.Metadata.PropagationContext?.ActivityContext,
                    Kind = ActivityKind.Consumer
                },
                token
            );
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}",e.Message, e.StackTrace);
        }
    }
}
