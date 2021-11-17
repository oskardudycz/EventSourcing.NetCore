using System;
using System.Threading;
using System.Threading.Tasks;
using Core.ElasticSearch.Indices;
using Core.Events;
using Core.Projections;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Nest;

namespace Core.ElasticSearch.Projections;

public class ElasticSearchProjection<TEvent, TView> : IEventHandler<TEvent>
    where TView : class, IProjection
    where TEvent : IEvent
{
    private readonly IElasticClient elasticClient;
    private readonly Func<TEvent, string> getId;

    public ElasticSearchProjection(
        IElasticClient elasticClient,
        Func<TEvent, string> getId
    )
    {
        this.elasticClient = elasticClient ?? throw new ArgumentNullException(nameof(elasticClient));
        this.getId = getId ?? throw new ArgumentNullException(nameof(getId));
    }

    public async Task Handle(TEvent @event, CancellationToken ct)
    {
        string id = getId(@event);

        var entity = (await elasticClient.GetAsync<TView>(id, ct: ct))?.Source
                     ?? (TView) Activator.CreateInstance(typeof(TView), true)!;

        entity.When(@event);

        var result = await elasticClient.UpdateAsync<TView>(id,
            u => u.Doc(entity).Upsert(entity).Index(IndexNameMapper.ToIndexName<TView>()),
            ct
        );
    }
}

public static class ElasticSearchProjectionConfig
{
    public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services,
        Func<TEvent, string> getId)
        where TView : class, IProjection
        where TEvent : IEvent
    {
        services.AddTransient<INotificationHandler<TEvent>>(sp =>
        {
            var session = sp.GetRequiredService<IElasticClient>();

            return new ElasticSearchProjection<TEvent, TView>(session, getId);
        });

        return services;
    }
}