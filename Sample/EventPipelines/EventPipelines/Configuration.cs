using Microsoft.Extensions.DependencyInjection;

namespace EventPipelines;

public static class Configuration
{
    public static IServiceCollection AddEventBus(this IServiceCollection services) =>
        services.AddSingleton<IEventBus, EventBus>();

    public static IServiceCollection AddEventHandler<T>(this IServiceCollection services, T eventHandler)
        where T : class, IEventHandler =>
        services.AddScoped<IEventHandler>(_ => eventHandler);

    public static IServiceCollection AddEventHandler<T>(this IServiceCollection services)
        where T : class, IEventHandler =>
        services.AddScoped<IEventHandler, T>();

    public static IServiceCollection Handle<TEvent>(
        this IServiceCollection services,
        Action<TEvent> handler) =>
        services.AddEventHandler(new EventHandlerWrapper<TEvent>(handler));

    public static IServiceCollection Handle<TEvent>(
        this IServiceCollection services,
        Func<TEvent, CancellationToken, ValueTask> handler) =>
        services.AddEventHandler(new EventHandlerWrapper<TEvent>(handler));

    public static IServiceCollection Handle<TEvent>(
        this IServiceCollection services,
        IEventHandler<TEvent> handler) =>
        services.AddEventHandler(handler);

    public static IServiceCollection Transform<TEvent, TTransformedEvent>(
        this IServiceCollection services,
        Func<TEvent, TTransformedEvent> handler) =>
        services.AddEventHandler(new EventTransformationWrapper<TEvent, TTransformedEvent>(handler));


    public static IServiceCollection Transform<TEvent>(
        this IServiceCollection services,
        Func<TEvent, object> handler) =>
        services.AddEventHandler(new EventTransformationWrapper<TEvent>(handler));

    public static IServiceCollection Transform<TEvent, TTransformedEvent>(
        this IServiceCollection services,
        Func<TEvent, CancellationToken, ValueTask<TTransformedEvent>> handler) =>
        services.AddEventHandler(new EventTransformationWrapper<TEvent, TTransformedEvent>(handler));


    public static IServiceCollection Transform<TEvent>(
        this IServiceCollection services,
        Func<TEvent, CancellationToken, ValueTask<object>> handler) =>
        services.AddEventHandler(new EventTransformationWrapper<TEvent>(handler));

    public static IServiceCollection Transform<TEvent, TTransformedEvent>(
        this IServiceCollection services,
        IEventTransformation<TEvent, TTransformedEvent> handler) =>
        services.AddEventHandler(handler);

    public static IServiceCollection Filter<TEvent>(
        this IServiceCollection services,
        Func<TEvent, bool> handler) =>
        services.AddEventHandler(new EventFilterWrapper<TEvent>(handler));

    public static IServiceCollection Filter<TEvent>(
        this IServiceCollection services,
        Func<TEvent, CancellationToken, ValueTask<bool>> handler) =>
        services.AddEventHandler(new EventFilterWrapper<TEvent>(handler));

    public static IServiceCollection Filter<TEvent>(
        this IServiceCollection services,
        IEventFilter<TEvent> handler) =>
        services.AddEventHandler(handler);
}
