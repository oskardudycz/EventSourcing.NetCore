using Core.Storage;
using Marten;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Tickets.Reservations.Commands;

namespace Tickets.Reservations
{
    internal static class Config
    {

        internal static void AddReservations(this IServiceCollection services)
        {
            services.AddScoped<IReservationNumberGenerator, ReservationNumberGenerator>();

            services.AddScoped<IRepository<Reservation>, MartenRepository<Reservation>>();

            services.AddScoped<IRequestHandler<CreateTentativeReservation, Unit>, ReservationCommandHandler>();
        }

        internal static void ConfigureReservations(this StoreOptions options)
        {
            options.Events.InlineProjections.AggregateStreamsWith<Reservation>();
            //options.Events.InlineProjections.Add<MeetingViewProjection>();
        }
    }
}
