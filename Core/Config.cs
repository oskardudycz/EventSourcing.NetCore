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
