using System;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Core.Reflection;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.Events.External.Kafka
{
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

            config = configuration.GetSection("KafkaConsumer").Get<KafkaConsumerConfig>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Kafka consumer started");

            using (var consumer = new ConsumerBuilder<string, string>(config.ConsumerConfig).Build())
            {
                consumer.Subscribe(config.Topics);
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await ConsumeMessage(consumer, cancellationToken);
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

        private async Task ConsumeMessage(IConsumer<string, string> consumer, CancellationToken cancellationToken)
        {
            try
            {
                var message = consumer.Consume(cancellationToken);

                var eventType = TypeProvider.GetTypeFromAnyReferencingAssembly(message.Key);

                var @event = JsonConvert.DeserializeObject(message.Value, eventType);

                using (var scope = serviceProvider.CreateScope())
                {
                    var mediator =
                        scope.ServiceProvider.GetRequiredService<IMediator>();

                    await mediator.Publish(@event);
                }
            }
            catch (Exception e)
            {
                logger.LogInformation("Error consuming message: " + e.Message + e.StackTrace);
            }
        }
    }
}
