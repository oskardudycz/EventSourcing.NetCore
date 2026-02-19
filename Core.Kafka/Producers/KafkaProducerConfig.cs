using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Producers;

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string Topic { get; set; } = null!;
    public int? ProducerTimeoutInMs { get; set; }
}

public static class KafkaProducerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaProducer";

    public static KafkaProducerConfig GetKafkaProducerConfig(this IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("kafka");
        var kafkaProducerConfig = configuration.GetRequiredConfig<KafkaProducerConfig>(DefaultConfigKey);

        if (connectionString == null) return kafkaProducerConfig;

        kafkaProducerConfig.ProducerConfig ??= new ProducerConfig();
        kafkaProducerConfig.ProducerConfig.BootstrapServers = connectionString;

        return kafkaProducerConfig;
    }
}
