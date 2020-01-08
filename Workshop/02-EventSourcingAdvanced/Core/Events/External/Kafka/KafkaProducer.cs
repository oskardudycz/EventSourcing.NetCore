using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Core.Events.External.Kafka
{
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

        public async Task Publish(IExternalEvent @event)
        {
            using (var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build())
            {
                await Task.Yield();
                // publish event to kafka topic taken from config
                await p.ProduceAsync(config.Topic,
                    new Message<string, string>
                    {
                        // store event type name in message Key
                        Key = @event.GetType().Name,
                        // serialize event to message Value
                        Value = JsonConvert.SerializeObject(@event)
                    });
            }
        }
    }
}
