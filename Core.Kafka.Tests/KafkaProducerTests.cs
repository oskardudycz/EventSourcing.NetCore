using Core.Events;
using Core.Kafka.Producers;
using Core.OpenTelemetry;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Core.Kafka.Tests;

public record TestEvent(string SomeValue, int AndAnother);

public class KafkaProducerTests
{
    private readonly IConfiguration kafkaConfig = new ConfigurationBuilder()
        .AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                { "KafkaProducer:ProducerConfig:BootstrapServers", "localhost:9092" },
                { "KafkaProducer:Topic", "MeetingsManagement" },
            }
        )
        .Build();

    private readonly IEventEnvelope eventEnvelope =
        new EventEnvelope<TestEvent>(new TestEvent("test", 123), new EventMetadata("123", 1, 1, null));

    [Fact]
    [Trait("Category", "SkipCI")]
    public async Task TaskShouldProduceMessage()
    {
        var kafkaProducer = new KafkaProducer(kafkaConfig, new ActivityScope(), null, new Logger<KafkaProducer>(LoggerFactory.Create(_ => { })));

        var exception = await Record.ExceptionAsync(async () =>
            await kafkaProducer.Publish(eventEnvelope, CancellationToken.None)
        );
        exception.Should().BeNull();
    }
}
