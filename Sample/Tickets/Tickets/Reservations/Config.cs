using Core.Storage;
using Marten;
using Marten.Pagination;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Reservations.Commands;
using Tickets.Reservations.Projections;
using Tickets.Reservations.Queries;

namespace Tickets.Reservations
{
    internal static class Config
    {

        internal static void AddReservations(this IServiceCollection services)
        {
            services.AddScoped<IReservationNumberGenerator, ReservationNumberGenerator>();

            services.AddScoped<IRepository<Reservation>, MartenRepository<Reservation>>();

            services.AddScoped<IRequestHandler<CreateTentativeReservation, Unit>, ReservationCommandHandler>();
            services.AddScoped<IRequestHandler<GetReservationById, ReservationDetails>, ReservationQueryHandler>();
            services.AddScoped<IRequestHandler<GetReservations, IPagedList<ReservationShortInfo>>, ReservationQueryHandler>();
            services.AddScoped<IRequestHandler<GetReservationHistory, IPagedList<ReservationHistory>>, ReservationQueryHandler>();
        }

        internal static void ConfigureReservations(this StoreOptions options)
        {
            options.Events.InlineProjections.AggregateStreamsWith<Reservation>();
            options.Events.InlineProjections.Add<ReservationDetailsProjection>();
            options.Events.InlineProjections.Add<ReservationShortInfoProjection>();
            options.Events.InlineProjections.TransformEvents(new ReservationHistoryTransformation());
        }
    }
}
