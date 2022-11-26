using Core.Commands;
using Core.Events;
using Core.Ids;
using Core.OpenTelemetry;
using Core.Queries;
using Core.Requests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class Config
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services
            .AddSingleton<IActivityScope, ActivityScope>()
            .AddEventBus()
            .AddCommandBus()
            .AddQueryBus();

        services.TryAddScoped<IExternalCommandBus, ExternalCommandBus>();

        services.TryAddScoped<IIdGenerator, NulloIdGenerator>();

        return services;
    }
}
