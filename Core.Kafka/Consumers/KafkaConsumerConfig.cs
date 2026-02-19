using Confluent.Kafka;
using Core.Configuration;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Consumers;

public class KafkaConsumerConfig
{
    public ConsumerConfig ConsumerConfig { get; set; } = null!;
    public string[] Topics { get; set; } = null!;

    public bool IgnoreDeserializationErrors { get; set; } =  true;
}

public static class KafkaConsumerConfigExtensions
{
    private const string DefaultConfigKey = "KafkaConsumer";

    public static KafkaConsumerConfig GetKafkaConsumerConfig(this IConfiguration configuration)
    {
        // Manually bind the ConsumerConfig until https://github.com/dotnet/runtime/issues/96652 is fixed
        var config = configuration.GetSection("kafka").GetSection("Config").Get<KafkaConsumerConfig>();

        var connectionString = configuration.GetConnectionString("kafka");
        var kafkaProducerConfig = configuration.GetRequiredConfig<KafkaConsumerConfig>(DefaultConfigKey);

        if (connectionString == null) return kafkaProducerConfig;

        kafkaProducerConfig.ConsumerConfig.BootstrapServers = connectionString;
        kafkaProducerConfig.ConsumerConfig.AllowAutoCreateTopics = true; //TODO: fix that!
        kafkaProducerConfig.ConsumerConfig.SecurityProtocol = SecurityProtocol.Plaintext;
        kafkaProducerConfig.ConsumerConfig.SaslMechanism = SaslMechanism.Plain;

        return kafkaProducerConfig;
    }
}
