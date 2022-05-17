using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Kafka.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Core.Kafka.Consumers;

public class KafkaConsumer: IExternalEventConsumer
{
    private readonly ILogger<KafkaConsumer> logger;
    private readonly IEventBus eventBus;
    private readonly KafkaConsumerConfig config;

    public KafkaConsumer(
        ILogger<KafkaConsumer> logger,
        IConfiguration configuration,
        IEventBus eventBus
    )
    {
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        this.eventBus = eventBus;

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

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

    private async Task ConsumeNextEvent(IConsumer<string, string> consumer, CancellationToken cancellationToken)
    {
        try
        {
            //lol ^_^ - remove this hack when this GH issue is solved: https://github.com/dotnet/extensions/issues/2149#issuecomment-518709751
            await Task.Yield();
            // wait for the upcoming message, consume it when arrives
            var message = consumer.Consume(cancellationToken);

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

            // publish event to internal event bus
            await eventBus.Publish(eventEnvelope, cancellationToken);

            consumer.Commit();
        }
        catch (Exception e)
        {
            logger.LogError("Error producing Kafka message: {Message} {StackTrace}",e.Message, e.StackTrace);
        }
    }
}
