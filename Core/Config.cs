using Core.Commands;
using Core.Events;
using Core.Extensions;
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
            .AllowResolvingKeyedServicesAsDictionary()
            .AddSingleton(TimeProvider.System)
            .AddSingleton(ActivityScope.Instance)
            .AddEventBus()
            .AddInMemoryCommandBus()
            .AddQueryBus();

        services.TryAddScoped<IExternalCommandBus, ExternalCommandBus>();

        services.TryAddScoped<IIdGenerator, NulloIdGenerator>();
        services.TryAddSingleton(EventTypeMapper.Instance);

        return services;
    }
}
