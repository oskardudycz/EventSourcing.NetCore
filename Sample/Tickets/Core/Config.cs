using Core.Commands;
using Core.Events;
using Core.Ids;
using Core.Queries;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core
{
    public static class Config
    {
        public static void AddCoreServices(this IServiceCollection services)
        {
            services.AddMediatR();

            services.AddScoped<ICommandBus, CommandBus>();
            services.AddScoped<IQueryBus, QueryBus>();
            services.TryAddScoped<IEventBus, EventBus>();

            services.AddScoped<IIdGenerator, MartenIdGenerator>();
        }

        private static void AddMediatR(this IServiceCollection services)
        {
            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<ServiceFactory>(sp => sp.GetService);
        }
    }
}
