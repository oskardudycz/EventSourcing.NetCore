using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Newtonsoft.Json;

namespace Core.Events
{
    public class KafkaProducer
    {
        private readonly string endpoint;
        private readonly string topic;
        private readonly string partitionKey;

        public KafkaProducer(
            string endpoint,
            string topic
        )
        {
            this.endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
            this.topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public async Task Publish(object @event)
        {
            var producerConfig = new ProducerConfig { BootstrapServers = endpoint };

            using (var p = new ProducerBuilder<Null, string>(producerConfig).Build())
            {
                while (true)
                {
                    try
                    {
                        await p.ProduceAsync(topic, new Message<Null, string> { Value = JsonConvert.SerializeObject(@event) });
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw e;
                    }
                }
            }
        }
    }
}
