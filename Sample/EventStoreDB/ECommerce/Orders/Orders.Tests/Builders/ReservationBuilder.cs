// using System;
// using Core.Aggregates;
// using Orders.Reservations;
// using Orders.Tests.Stubs.Ids;
// using Orders.Tests.Stubs.Reservations;
//
// namespace Orders.Tests.Builders
// {
//     internal class ReservationBuilder
//     {
//         private Func<Reservation> build  = () => null;
//
//         public ReservationBuilder Tentative()
//         {
//             var reservationId = Guid.NewGuid();
//             var numberGenerator = new FakeReservationNumberGenerator();
//             var seatId = Guid.NewGuid();
//
//             // When
//             var reservation = Reservation.CreateTentative(
//                 reservationId,
//                 numberGenerator,
//                 seatId
//             );
//
//             build = () => reservation;
//
//             return this;
//         }
//
//         public static ReservationBuilder Create() => new ReservationBuilder();
//
//         public Reservation Build()
//         {
//             var reservation = build();
//             ((IAggregate)reservation).DequeueUncommittedEvents();
//             return reservation;
//         }
//     }
// }
