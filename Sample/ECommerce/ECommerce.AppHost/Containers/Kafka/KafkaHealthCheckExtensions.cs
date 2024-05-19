// Taken from: https://github.com/davidfowl/WaitForDependenciesAspire/blob/main/WaitForDependencies.Aspire.Hosting/PostgreSqlHealthCheckExtensions.cs

using Confluent.Kafka;
using HealthChecks.Kafka;

namespace Aspire.Hosting;

public static class KafkaHealthCheckExtensions
{
    /// <summary>
    /// Adds a health check to the PostgreSQL server resource.
    /// </summary>
    public static IResourceBuilder<KafkaServerResource> WithHealthCheck(this IResourceBuilder<KafkaServerResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(cs =>
            new KafkaHealthCheck(new KafkaHealthCheckOptions
            {
                Configuration = new ProducerConfig
                {
                    BootstrapServers = cs,
                    AllowAutoCreateTopics = true
                }
            })));
}
