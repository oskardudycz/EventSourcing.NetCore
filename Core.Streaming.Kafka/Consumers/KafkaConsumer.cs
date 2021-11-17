using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.Streaming.Kafka.Consumers;

public class KafkaConsumer: IExternalEventConsumer
{
    private readonly IServiceProvider serviceProvider;
    private readonly ILogger<KafkaConsumer> logger;
    private readonly KafkaConsumerConfig config;

    public KafkaConsumer(
        IServiceProvider serviceProvider,
        ILogger<KafkaConsumer> logger,
        IConfiguration configuration
    )
    {
        this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // get configuration from appsettings.json
        config = configuration.GetKafkaConsumerConfig();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Kafka consumer started");

        // create consumer
        using (var consumer = new ConsumerBuilder<string, string>(config.ConsumerConfig).Build())
        {
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
                logger.LogInformation("Error consuming message: " + e.Message + e.StackTrace);

                // Ensure the consumer leaves the group cleanly and final offsets are committed.
                consumer.Close();
            }
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

            // get event type from name storred in message.Key
            var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(message.Message.Key)!;

            // deserialize event
            var @event = JsonConvert.DeserializeObject(message.Message.Value, eventType)!;

            using (var scope = serviceProvider.CreateScope())
            {
                var eventBus =
                    scope.ServiceProvider.GetRequiredService<IEventBus>();

                // publish event to internal event bus
                await eventBus.Publish((IEvent)@event);
            }
        }
        catch (Exception e)
        {
            logger.LogInformation("Error consuming message: " + e.Message + e.StackTrace);
        }
    }
}