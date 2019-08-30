using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Events.External.Kafka;
using Core.Queries;
using MeetingsManagement.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core
{
    public static class Config
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddMediatR();

            //using TryAdd to support mocking, without that it won't be possible to override in tests
            services.TryAddScoped<IExternalEventProducer, KafkaProducer>();
            services.TryAddSingleton<IExternalEventConsumer, KafkaConsumer>();
            services.AddHostedService<ExternalEventConsumerBackgroundWorker>();

            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
            services.AddScoped<IEventBus, EventBus>();
        }
    }
}
