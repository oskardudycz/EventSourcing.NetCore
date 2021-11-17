using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Queries;

public static class Config
{
    public static IServiceCollection AddQueryHandler<TQuery, TQueryResult, TQueryHandler>(
        this IServiceCollection services
    )
        where TQuery : IQuery<TQueryResult>
        where TQueryHandler : class, IQueryHandler<TQuery, TQueryResult>
    {
        return services.AddTransient<TQueryHandler>()
            .AddTransient<IRequestHandler<TQuery, TQueryResult>>(sp => sp.GetRequiredService<TQueryHandler>())
            .AddTransient<IQueryHandler<TQuery, TQueryResult>>(sp => sp.GetRequiredService<TQueryHandler>());
    }
}