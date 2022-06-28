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
    public async Task Post_ShouldReturn_CreatedStatus_With_CartId()
    {
        var createdReservationId = Guid.Empty;

        await API.Scenario(
            // Create Reservations
            API.Given(
                    URI("/api/Reservations/"),
                    BODY(new CreateTentativeReservationRequest { SeatId = SeatId })
                )
                .When(POST)
                .Then(CREATED_WITH_DEFAULT_HEADERS(eTag: 1),
                    response =>
                    {
                        createdReservationId = response.GetCreatedId<Guid>();
                        return ValueTask.CompletedTask;
                    }),

            // Get reservation details
            _ => API.Given(
                    URI($"/api/Reservations/{createdReservationId}")
                )
                .When(GET)
                .Then(
                    OK,
                    RESPONSE_BODY<ReservationDetails>(reservation =>
                    {
                        reservation.Id.Should().Be(createdReservationId);
                        reservation.Status.Should().Be(ReservationStatus.Tentative);
                        reservation.SeatId.Should().Be(SeatId);
                        reservation.Number.Should().NotBeEmpty();
                        reservation.Version.Should().Be(1);
                    })),

            // Get reservations list
            _ => API.Given(
                    URI("/api/Reservations/")
                )
                .When(GET)
                .Then(
                    OK,
                    RESPONSE_BODY<PagedListResponse<ReservationShortInfo>>(reservations =>
                    {
                        reservations.Should().NotBeNull();
                        reservations.Items.Should().NotBeNull();

                        reservations.Items.Should().HaveCount(1);
                        reservations.TotalItemCount.Should().Be(1);
                        reservations.HasNextPage.Should().Be(false);

                        var reservationInfo = reservations.Items.Single();

                        reservationInfo.Id.Should().Be(createdReservationId);
                        reservationInfo.Number.Should().NotBeNull().And.NotBeEmpty();
                        reservationInfo.Status.Should().Be(ReservationStatus.Tentative);
                    })),

            // Get reservation history
            _ => API.Given(
                    URI($"/api/Reservations/{createdReservationId}/history")
                )
                .When(GET)
                .Then(
                    OK,
                    RESPONSE_BODY<PagedListResponse<ReservationHistory>>(reservations =>
                    {
                        reservations.Should().NotBeNull();
                        reservations.Items.Should().NotBeNull();

                        reservations.Items.Should().HaveCount(1);
                        reservations.TotalItemCount.Should().Be(1);
                        reservations.HasNextPage.Should().Be(false);

                        var reservationInfo = reservations.Items.Single();

                        reservationInfo.ReservationId.Should().Be(createdReservationId);
                        reservationInfo.Description.Should().StartWith("Created tentative reservation with number");
                    }))
        );
    }

    private readonly Guid SeatId = Guid.NewGuid();

    private readonly ApiSpecification<Program> API;

    public CreateTentativeReservationTests(TestWebApplicationFactory<Program> fixture) =>
        API = ApiSpecification<Program>.Setup(fixture);
}
