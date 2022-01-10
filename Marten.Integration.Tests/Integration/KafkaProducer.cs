using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Confluent.Kafka;
using Marten.Events;
using Newtonsoft.Json;

namespace Marten.Integration.Tests.Integration;

public class KafkaProducer: IMartenEventsConsumer
{
    private readonly KafkaProducerConfig config;

    public KafkaProducer(KafkaProducerConfig config)
    {
        this.config = config;
    }

    public async Task ConsumeAsync(IReadOnlyList<StreamAction> streamActions)
    {
        using var kafkaProducer = new ProducerBuilder<string, string>(config.ProducerConfig).Build();

        foreach (var @event in streamActions.SelectMany(streamAction => streamAction.Events))
        {
            await kafkaProducer.ProduceAsync(config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.GetType().Name,
                    // serialize event to message Value
                    Value = JsonConvert.SerializeObject(@event)
                });
        }
    }
}

public class KafkaProducerConfig
{
    public ProducerConfig? ProducerConfig { get; set; }
    public string? Topic { get; set; }
}
