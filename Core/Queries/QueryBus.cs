using Core.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Polly;

namespace Core.Queries;

public class QueryBus: IQueryBus
{
    private readonly IServiceProvider serviceProvider;
    private readonly AsyncPolicy retryPolicy;
    private readonly IActivityScope activityScope;

    public QueryBus(
        IServiceProvider serviceProvider,
        IActivityScope activityScope,
        AsyncPolicy retryPolicy
    )
    {
        this.serviceProvider = serviceProvider;
        this.retryPolicy = retryPolicy;
        this.activityScope = activityScope;
    }

    public Task<TResponse> Query<TQuery, TResponse>(TQuery query, CancellationToken ct = default)
        where TQuery : notnull
    {
        var queryHandler =
            serviceProvider.GetService<IQueryHandler<TQuery, TResponse>>()
            ?? throw new InvalidOperationException($"Unable to find handler for Query '{query.GetType().Name}'");

        var queryName = typeof(TQuery).Name;
        var activityName = $"{queryHandler.GetType().Name}/{queryName}";

        return activityScope.Run(
            activityName,
            (_, token) => retryPolicy.ExecuteAsync(c => queryHandler.Handle(query, c), token),
            new StartActivityOptions { Tags = {{ TelemetryTags.QueryHandling.Query, queryName }}},
            ct
        );
    }
}

public static class EventBusExtensions
{
    public static IServiceCollection AddQueryBus(this IServiceCollection services, AsyncPolicy? asyncPolicy = null)
    {
        services
            .AddScoped(sp =>
                new QueryBus(
                    sp,
                    sp.GetRequiredService<IActivityScope>(),
                    asyncPolicy ?? Policy.NoOpAsync()
                ))
            .TryAddScoped<IQueryBus>(sp =>
                sp.GetRequiredService<QueryBus>()
            );

        return services;
    }
}
