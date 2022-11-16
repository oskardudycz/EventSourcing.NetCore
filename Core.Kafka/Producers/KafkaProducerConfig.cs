using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Producers;

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string? Topic { get; set; }
}

public static class KafkaProducerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaProducer";

    public static KafkaProducerConfig GetKafkaProducerConfig(this IConfiguration configuration) =>
        configuration.GetRequiredConfig<KafkaProducerConfig>(DefaultConfigKey);
}
