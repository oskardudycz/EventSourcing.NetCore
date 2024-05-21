﻿using Core.Events;
using Core.EventStoreDB;
using Core.EventStoreDB.Subscriptions.Checkpoints.Postgres;
using Core.Extensions;
using Core.OpenTelemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Core;

public static class Configuration
{
    public static IServiceCollection AddCoreServices(
        this IServiceCollection services,
        IConfiguration configuration
    ) =>
        services
            .AllowResolvingKeyedServicesAsDictionary()
            .AddSingleton<IActivityScope, ActivityScope>()
            .AddEventBus()
            .AddEventStoreDB(configuration)
            .AddPostgresCheckpointing();
}
