using Confluent.Kafka;
using Core.Events;
using Core.Events.External;
using Core.Serialization.Newtonsoft;
using Core.Threading;
using Microsoft.Extensions.Configuration;

namespace Core.Kafka.Producers;

public class KafkaProducer: IExternalEventProducer
{
    private readonly KafkaProducerConfig config;

    public KafkaProducer(
        IConfiguration configuration
    )
    {
        if (configuration == null)
            throw new ArgumentNullException(nameof(configuration));

        // get configuration from appsettings.json
        config = configuration.GetKafkaProducerConfig();
    }

    public async Task Publish(EventEnvelope @event, CancellationToken ct)
    {
        try
        {
            using var p = new ProducerBuilder<string, string>(config.ProducerConfig).Build();
            await Task.Yield();
            // publish event to kafka topic taken from config

            // var result = p.ProduceAsync(config.Topic,
            //     new Message<string, string>
            //     {
            //         // store event type name in message Key
            //         Key = @event.GetType().Name,
            //         // serialize event to message Value
            //         Value = @event.ToJson()
            //     }, ct).ConfigureAwait(false).GetAwaiter().GetResult();

            p.Produce(config.Topic,
                new Message<string, string>
                {
                    // store event type name in message Key
                    Key = @event.GetType().Name,
                    // serialize event to message Value
                    Value = @event.ToJson()
                });
            p.Flush(ct);
            // Console.WriteLine(result);
            // var result = p.ProduceAsync(config.Topic,
            //     new Message<string, string>
            //     {
            //         // store event type name in message Key
            //         Key = @event.GetType().Name,
            //         // serialize event to message Value
            //         Value = @event.ToJson()
            //     }, ct).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}
