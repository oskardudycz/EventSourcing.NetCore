using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Marten.ExternalProjections
{
    public class MartenExternalProjection<TEvent, TView>: IEventHandler<TEvent>
        where TView: notnull
        where TEvent: IEvent
    {
        private readonly IDocumentSession session;
        private readonly Func<TEvent, Guid> getId;
        private readonly Action<TEvent, TView> apply;

        public MartenExternalProjection(
            IDocumentSession session,
            Func<TEvent, Guid> getId,
            Action<TEvent, TView> apply
        )
        {
            this.session = session ?? throw new ArgumentNullException(nameof(session));
            this.getId = getId ?? throw new ArgumentNullException(nameof(getId));
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
        }

        public async Task Handle(TEvent @event, CancellationToken ct)
        {
            var entity = (await session.LoadAsync<TView>(getId(@event), ct))
                         ?? (TView)Activator.CreateInstance(typeof(TView), true)!;

            apply(@event, entity);

            session.Store(entity);

            await session.SaveChangesAsync(ct);
        }
    }

    public static class MartenExternalProjectionConfig
    {
        public static IServiceCollection Project<TEvent, TView>(this IServiceCollection services, Func<TEvent, Guid> getId, Action<TEvent, TView> apply)
            where TView: notnull
            where TEvent: IEvent
        {
            services.AddTransient<INotificationHandler<TEvent>>(sp =>
            {
                var session = sp.GetRequiredService<IDocumentSession>();

                return new MartenExternalProjection<TEvent, TView>(session, getId, apply);
            });

            return services;
        }
    }
}
