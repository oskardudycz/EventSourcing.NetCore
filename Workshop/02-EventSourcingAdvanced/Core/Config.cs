using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Events.External.Kafka;
using Core.Queries;
using MeetingsManagement.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public static class Config
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddMediatR();

            services.AddScoped<IExternaEventProducer, KafkaProducer>();
            services.AddSingleton<IExternalEventConsumer, KafkaConsumer>();
            services.AddHostedService<ExternalEventConsumerBackgroundWorker>();

            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
            services.AddScoped<IEventBus, EventBus>();
        }
    }
}
