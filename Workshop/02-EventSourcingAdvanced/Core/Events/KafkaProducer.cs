using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace Core.Events
{
    public class KafkaProducer: IKafkaProducer
    {
        private readonly string topic;
        private readonly ProducerConfig producerConfig;

        public KafkaProducer(
            IConfiguration config
        )
        {
            var kafkaConfig = config.GetSection("KafkaProducer");
            topic = kafkaConfig.GetSection("Topic").Value ?? throw new ArgumentNullException(nameof(topic));
            var endpoint = kafkaConfig.GetSection("Endpoint").Value;

            producerConfig = new ProducerConfig { BootstrapServers = endpoint };
        }

        public async Task Publish(object @event)
        {
            using (var p = new ProducerBuilder<string, string>(producerConfig).Build())
            {
                await p.ProduceAsync(topic,
                    new Message<string, string>
                    {
                        Key = @event.GetType().Name,
                        Value = JsonConvert.SerializeObject(@event)
                    });
            }
        }
    }
}
