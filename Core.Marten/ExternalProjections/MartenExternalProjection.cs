using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.Projections;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.ExternalProjections
{
    public class MartenExternalProjection<TEvent, TView>: IEventHandler<TEvent>
        where TView: IProjection
        where TEvent: IEvent
    {
        private readonly IDocumentSession session;
        private readonly Func<TEvent, Guid> getId;

        public MartenExternalProjection(
            IDocumentSession session,
            Func<TEvent, Guid> getId
        )
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.getId = getId ?? throw new ArgumentNullException(nameof(getId));
        }

        public async Task Handle(TEvent @event, CancellationToken ct)
        {
            var entity = (await session.LoadAsync<TView>(getId(@event), ct))
                         ?? (TView)Activator.CreateInstance(typeof(TView), true)!;

            entity.When(@event);

            session.Store(entity);

            await session.SaveChangesAsync(ct);
        }
    }

    public static class MartenExternalProjectionConfig
    {
        public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services, Func<TEvent, Guid> getId)
            where TView: IProjection
            where TEvent: IEvent
        {
            services.AddTransient<INotificationHandler<TEvent>>(sp =>
            {
                var session = sp.GetRequiredService<IDocumentSession>();

                return new MartenExternalProjection<TEvent, TView>(session, getId);
            });

            return services;
        }
    }
}
