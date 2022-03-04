using Core.Commands;
using Core.Events;
using Core.Events.External;
using Core.Ids;
using Core.Queries;
using Core.Requests;
using Core.Tracing.Correlation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Core;

public static class Config
{
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        services.AddMediatR()
            .AddScoped<ICommandBus, CommandBus>()
            .AddScoped<IQueryBus, QueryBus>()
            .AddScoped<IEventMetadataProvider, EventMetadataProvider>()
            .AddScoped<ICorrelationIdProvider, CorrelationIdProvider>();

        services.TryAddScoped<IEventBus, EventBus>();
        services.TryAddScoped<IExternalEventProducer, NulloExternalEventProducer>();
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
