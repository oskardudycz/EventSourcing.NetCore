using Confluent.Kafka;
using Microsoft.Extensions.Configuration;

namespace Core.Streaming.Kafka.Producers;

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string? Topic { get; set; }
}

public static class KafkaProducerConfigExtensions
{
    public const string DefaultConfigKey = "KafkaProducer";

    public static KafkaProducerConfig GetKafkaProducerConfig(this IConfiguration configuration)
    {
        return configuration.GetSection(DefaultConfigKey).Get<KafkaProducerConfig>();
    }
}