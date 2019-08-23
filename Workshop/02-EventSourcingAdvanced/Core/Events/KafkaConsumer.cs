using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Core.Events
{
    public class KafkaConsumer: IHostedService
    {
        private readonly IServiceProvider serviceProvider;
        private readonly ILogger<KafkaConsumer> logger;
        private readonly string[] topics;
        private readonly ConsumerConfig consumerConfig;

        public KafkaConsumer(
            IServiceProvider serviceProvider,
            ILogger<KafkaConsumer> logger,
            IConfiguration config
        )
        {
            this.serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var kafkaConfig = config.GetSection("KafkaConsumer");
            var consumerGroup = kafkaConfig.GetSection("ConsumerGroup").Value;
            var endpoint = kafkaConfig.GetSection("Endpoint").Value;

            topics = kafkaConfig.GetSection("Topics").Value.Split(";");

            consumerConfig = new ConsumerConfig
            {
                GroupId = consumerGroup,
                BootstrapServers = endpoint,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Kafka consumer started");

            using (var consumer = new ConsumerBuilder<string, string>(consumerConfig).Build())
            {
                consumer.Subscribe(topics);
                try
                {
                    while (true)
                    {
                        try
                        {
                            var message = consumer.Consume();

                            var referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies().Select(a => a.FullName);
                            var type = AppDomain.CurrentDomain.GetAssemblies()
                                .Where(a => referencedAssemblies.Contains(a.FullName))
                                .SelectMany(a => a.GetTypes().Where(x => x.Name == message.Key))
                                .FirstOrDefault();

                            var @event = JsonConvert.DeserializeObject(message.Value, type);

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
                catch (Exception e)
                {
                    logger.LogInformation("Error consuming message: " + e.Message + e.StackTrace);
                    // Ensure the consumer leaves the group cleanly and final offsets are committed.
                    consumer.Close();
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            logger.LogInformation("Kafka consumer stoped");

            return Task.CompletedTask;
        }
    }
}
