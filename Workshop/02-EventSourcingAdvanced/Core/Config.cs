using Core.Commands;
using Core.Events;
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

            services.AddScoped<IKafkaProducer, KafkaProducer>();
            services.AddHostedService<KafkaConsumer>();

            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
            services.AddScoped<IEventBus, EventBus>();
        }
    }
}
