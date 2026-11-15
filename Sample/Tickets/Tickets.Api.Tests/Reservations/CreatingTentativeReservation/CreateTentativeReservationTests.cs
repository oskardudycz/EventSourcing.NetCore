using Core.Testing;
using Shouldly;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Reservations;
using Tickets.Reservations.GettingReservationById;
using Tickets.Reservations.GettingReservationHistory;
using Tickets.Reservations.GettingReservations;
using Ogooreck.API;
using Xunit;
using static Ogooreck.API.ApiSpecification;

namespace Tickets.Api.Tests.Reservations.CreatingTentativeReservation;

public class CreateTentativeReservationTests
{
    [Fact]
    public Task Post_ShouldReturn_CreatedStatus_With_CartId() =>
        API.Given()
            .When(
                POST,
                URI("/api/Reservations/"),
                BODY(new CreateTentativeReservationRequest { SeatId = SeatId })
            )
            .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1))
            .And()
            .When(GET, URI(ctx => $"/api/Reservations/{ctx.GetCreatedId()}"))
            .Then(
                OK,
                RESPONSE_BODY<ReservationDetails>((reservation, ctx) =>
                {
                    reservation.Id.ShouldBe(ctx.GetCreatedId<Guid>());
                    reservation.Status.ShouldBe(ReservationStatus.Tentative);
                    reservation.SeatId.ShouldBe(SeatId);
                    reservation.Number.ShouldNotBeEmpty();
                    reservation.Version.ShouldBe(1);
                })
            )
            .And()
            .When(GET, URI("/api/Reservations/"))
            .Then(
                OK,
                RESPONSE_BODY<PagedListResponse<ReservationShortInfo>>((reservations, ctx) =>
                {
                    reservations.ShouldNotBeNull();
                    reservations.Items.ShouldNotBeNull();

                    reservations.Items.Count().ShouldBe(1);
                    reservations.TotalItemCount.ShouldBe(1);
                    reservations.HasNextPage.ShouldBe(false);

                    var reservationInfo = reservations.Items.Single();

                    reservationInfo.Id.ShouldBe(ctx.GetCreatedId<Guid>());
                    reservationInfo.Number.ShouldNotBeNullOrEmpty();
                    reservationInfo.Status.ShouldBe(ReservationStatus.Tentative);
                }))
            .And()
            .When(GET, URI(ctx => $"/api/Reservations/{ctx.GetCreatedId()}/history"))
            .Then(
                OK,
                RESPONSE_BODY<PagedListResponse<ReservationHistory>>((reservations, ctx) =>
                {
                    reservations.ShouldNotBeNull();
                    reservations.Items.ShouldNotBeNull();

                    reservations.Items.Count().ShouldBe(1);
                    reservations.TotalItemCount.ShouldBe(1);
                    reservations.HasNextPage.ShouldBe(false);

                    var reservationInfo = reservations.Items.Single();

                    reservationInfo.ReservationId.ShouldBe(ctx.GetCreatedId<Guid>());
                    reservationInfo.Description.ShouldStartWith("Created tentative reservation with number");
                })
            );

    private readonly Guid SeatId = Guid.CreateVersion7();

    private readonly ApiSpecification<Program> API = ApiSpecification<Program>.Setup(
        new TestWebApplicationFactory<Program>()
    );
}
