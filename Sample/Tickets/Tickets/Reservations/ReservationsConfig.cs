using Core.Marten.Repository;
using Core.Repositories;
using Marten;
using Marten.Pagination;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Reservations.CancellingReservation;
using Tickets.Reservations.ChangingReservationSeat;
using Tickets.Reservations.ConfirmingReservation;
using Tickets.Reservations.CreatingTentativeReservation;
using Tickets.Reservations.GettingReservationAtVersion;
using Tickets.Reservations.GettingReservationById;
using Tickets.Reservations.GettingReservationHistory;
using Tickets.Reservations.GettingReservations;
using Tickets.Reservations.NumberGeneration;

namespace Tickets.Reservations
{
    internal static class ReservationsConfig
    {

        internal static void AddReservations(this IServiceCollection services)
        {
            services.AddScoped<IReservationNumberGenerator, ReservationNumberGenerator>();

            services.AddScoped<IRepository<Reservation>, MartenRepository<Reservation>>();

            AddCommandHandlers(services);
            AddQueryHandlers(services);
        }

        private static void AddCommandHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<CreateTentativeReservation, Unit>, HandleCreateTentativeReservation>();
            services.AddScoped<IRequestHandler<ChangeReservationSeat, Unit>, HandleChangeReservationSeat>();
            services.AddScoped<IRequestHandler<ConfirmReservation, Unit>, HandleConfirmReservation>();
            services.AddScoped<IRequestHandler<CancelReservation, Unit>, HandleCancelReservation>();
        }

        private static void AddQueryHandlers(IServiceCollection services)
        {
            services.AddScoped<IRequestHandler<GetReservationById, ReservationDetails>, HandleGetReservationById>();
            services.AddScoped<IRequestHandler<GetReservationAtVersion, ReservationDetails>, HandleGetReservationAtVersion>();
            services.AddScoped<IRequestHandler<GetReservations, IPagedList<ReservationShortInfo>>, HandleGetReservations>();
            services.AddScoped<IRequestHandler<GetReservationHistory, IPagedList<ReservationHistory>>, HandleGetReservationHistory>();
        }

        internal static void ConfigureReservations(this StoreOptions options)
        {
            // Snapshots
            options.Projections.SelfAggregate<Reservation>();
            options.Schema.For<Reservation>().Index(x => x.SeatId, x =>
            {
                x.IsUnique = true;

                // Partial index by supplying a condition
                x.Predicate = "(data ->> 'Status') != 'Cancelled'";
            });
            options.Schema.For<Reservation>().Index(x => x.Number, x =>
            {
                x.IsUnique = true;

                // Partial index by supplying a condition
                x.Predicate = "(data ->> 'Status') != 'Cancelled'";
            });


            // options.Schema.For<Reservation>().UniqueIndex(x => x.SeatId);

            // projections
            options.Projections.Add<ReservationDetailsProjection>();
            options.Projections.Add<ReservationShortInfoProjection>();

            // transformation
            options.Projections.Add<ReservationHistoryTransformation>();
        }
    }
}
