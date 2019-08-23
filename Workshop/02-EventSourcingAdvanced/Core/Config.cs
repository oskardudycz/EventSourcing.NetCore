using Core.Commands;
using Core.Events;
using Core.Queries;
using Microsoft.Extensions.DependencyInjection;

namespace Core
{
    public static class Config
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
            services.AddScoped<IEventBus, EventBus>();
        }
    }
}
