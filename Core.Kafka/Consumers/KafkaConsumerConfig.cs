using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Consumers;

public class KafkaConsumerConfig
{
    public ConsumerConfig? ConsumerConfig { get; set; }
    public string[]? Topics { get; set; }

    public bool IgnoreDeserializationErrors { get; set; } =  true;
}

public static class KafkaConsumerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaConsumer";

    public static KafkaConsumerConfig GetKafkaConsumerConfig(this IConfiguration configuration)
    {
        return configuration.GetRequiredConfig<KafkaConsumerConfig>(DefaultConfigKey);
    }
}
