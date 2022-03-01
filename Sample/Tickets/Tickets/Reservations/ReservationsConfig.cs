using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Marten;
using Marten.Pagination;
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

namespace Tickets.Reservations;

internal static class ReservationsConfig
{
    internal static void AddReservations(this IServiceCollection services)
    {
        services
            .AddScoped<IReservationNumberGenerator, ReservationNumberGenerator>()
            .AddScoped<IMartenRepository<Reservation>, MartenRepository<Reservation>>()
            .AddCommandHandlers()
            .AddQueryHandlers();
    }

    private static IServiceCollection AddCommandHandlers(this IServiceCollection services)
    {
        return services
            .AddCommandHandler<CreateTentativeReservation, HandleCreateTentativeReservation>()
            .AddCommandHandler<ChangeReservationSeat,HandleChangeReservationSeat>()
            .AddCommandHandler<ConfirmReservation, HandleConfirmReservation>()
            .AddCommandHandler<CancelReservation, HandleCancelReservation>();
    }

    private static IServiceCollection AddQueryHandlers(this IServiceCollection services)
    {
        return services
            .AddQueryHandler<GetReservationById, ReservationDetails, HandleGetReservationById>()
            .AddQueryHandler<GetReservationAtVersion, ReservationDetails, HandleGetReservationAtVersion>()
            .AddQueryHandler<GetReservations, IPagedList<ReservationShortInfo>, HandleGetReservations>()
            .AddQueryHandler<GetReservationHistory, IPagedList<ReservationHistory>, HandleGetReservationHistory>();
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
