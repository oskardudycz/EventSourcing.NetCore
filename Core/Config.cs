using Core.Commands;
using Core.Events;
using Core.Ids;
using Core.OpenTelemetry;
using Core.Queries;
using Core.Requests;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class Config
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        // TODO: Remove MediatR
        services
            .AddSingleton<IActivityScope, ActivityScope>()
            .AddMediatR()
            .AddScoped<IQueryBus, QueryBus>()
            .AddEventBus()
            .AddCommandBus();

        services.TryAddScoped<IExternalCommandBus, ExternalCommandBus>();

        services.TryAddScoped<IIdGenerator, NulloIdGenerator>();

        return services;
    }

    private static IServiceCollection AddMediatR(this IServiceCollection services)
    {
        return services
            .AddScoped<IMediator, Mediator>()
            .AddTransient<ServiceFactory>(sp => sp.GetRequiredService!);
    }
}
