using Confluent.Kafka;

namespace Core.Events.External.Kafka
{
    public class KafkaConsumerConfig
    {
        public ConsumerConfig ConsumerConfig { get; set; }
        public string[] Topics { get; set; }
    }
}
