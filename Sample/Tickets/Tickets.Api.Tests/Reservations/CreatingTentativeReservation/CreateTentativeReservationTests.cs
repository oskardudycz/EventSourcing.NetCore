using Core.Testing;
using FluentAssertions;
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

public class CreateTentativeReservationTests: IClassFixture<TestWebApplicationFactory<Program>>
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
                    reservation.Id.Should().Be(ctx.GetCreatedId<Guid>());
                    reservation.Status.Should().Be(ReservationStatus.Tentative);
                    reservation.SeatId.Should().Be(SeatId);
                    reservation.Number.Should().NotBeEmpty();
                    reservation.Version.Should().Be(1);
                })
            )
            .And()
            .When(GET, URI("/api/Reservations/"))
            .Then(
                OK,
                RESPONSE_BODY<PagedListResponse<ReservationShortInfo>>((reservations, ctx) =>
                {
                    reservations.Should().NotBeNull();
                    reservations.Items.Should().NotBeNull();

                    reservations.Items.Should().HaveCount(1);
                    reservations.TotalItemCount.Should().Be(1);
                    reservations.HasNextPage.Should().Be(false);

                    var reservationInfo = reservations.Items.Single();

                    reservationInfo.Id.Should().Be(ctx.GetCreatedId());
                    reservationInfo.Number.Should().NotBeNull().And.NotBeEmpty();
                    reservationInfo.Status.Should().Be(ReservationStatus.Tentative);
                }))
            .And()
            .When(GET, URI(ctx => $"/api/Reservations/{ctx.GetCreatedId()}/history"))
            .Then(
                OK,
                RESPONSE_BODY<PagedListResponse<ReservationHistory>>((reservations, ctx) =>
                {
                    reservations.Should().NotBeNull();
                    reservations.Items.Should().NotBeNull();

                    reservations.Items.Should().HaveCount(1);
                    reservations.TotalItemCount.Should().Be(1);
                    reservations.HasNextPage.Should().Be(false);

                    var reservationInfo = reservations.Items.Single();

                    reservationInfo.ReservationId.Should().Be(ctx.GetCreatedId<Guid>());
                    reservationInfo.Description.Should().StartWith("Created tentative reservation with number");
                })
            );

    private readonly Guid SeatId = Guid.NewGuid();

    private readonly ApiSpecification<Program> API;

    public CreateTentativeReservationTests(TestWebApplicationFactory<Program> fixture) =>
        API = ApiSpecification<Program>.Setup(fixture);
}
