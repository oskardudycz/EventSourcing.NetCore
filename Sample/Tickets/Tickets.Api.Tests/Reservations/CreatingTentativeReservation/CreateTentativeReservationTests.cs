using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Api.Tests.Config;
using Tickets.Reservations;
using Tickets.Reservations.CreatingTentativeReservation;
using Tickets.Reservations.GettingReservationById;
using Tickets.Reservations.GettingReservationHistory;
using Tickets.Reservations.GettingReservations;
using Xunit;
using Xunit.Abstractions;

namespace Tickets.Api.Tests.Reservations.CreatingTentativeReservation
{
    public class CreateTentativeReservationFixture: ApiWithEventsFixture<Startup>
    {
        protected override string ApiUrl => "/api/Reservations";

        protected override Dictionary<string, string> GetConfiguration(string fixtureName) =>
            TestConfiguration.Get(fixtureName);

        public readonly Guid SeatId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse = default!;

        protected override async Task Setup()
        {
            // send create command
            CommandResponse = await Post(new CreateTentativeReservationRequest {SeatId = SeatId});
        }
    }

    public class CreateTentativeReservationTests: ApiTest<CreateTentativeReservationFixture>
    {
        public CreateTentativeReservationTests(CreateTentativeReservationFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_ReservationId()
        {
            var commandResponse = Fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldPublish_TentativeReservationCreated()
        {
            var createdReservationId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            Fixture.PublishedInternalEventsOfType<TentativeReservationCreated>()
                .Should()
                .HaveCount(1)
                .And.Contain(@event =>
                    @event.ReservationId == createdReservationId
                    && @event.SeatId == Fixture.SeatId
                    && !string.IsNullOrEmpty(@event.Number)
                );
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldCreate_ReservationDetailsReadModel()
        {
            var createdReservationId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            // prepare query
            var query = $"{createdReservationId}";

            //send query
            var queryResponse = await Fixture.Get(query);
            queryResponse.EnsureSuccessStatusCode();

            var reservationDetails =  await queryResponse.GetResultFromJson<ReservationDetails>();
            reservationDetails.Id.Should().Be(createdReservationId);
            reservationDetails.Number.Should().NotBeNull().And.NotBeEmpty();
            reservationDetails.Status.Should().Be(ReservationStatus.Tentative);
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldCreate_ReservationList()
        {
            var createdReservationId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            //send query
            var queryResponse = await Fixture.Get();
            queryResponse.EnsureSuccessStatusCode();

            var reservationPagedList = await queryResponse.GetResultFromJson<PagedListResponse<ReservationShortInfo>>();

            reservationPagedList.Should().NotBeNull();
            reservationPagedList.Items.Should().NotBeNull();

            reservationPagedList.Items.Should().HaveCount(1);
            reservationPagedList.TotalItemCount.Should().Be(1);
            reservationPagedList.HasNextPage.Should().Be(false);

            var reservationInfo = reservationPagedList.Items.Single();

            reservationInfo.Id.Should().Be(createdReservationId);
            reservationInfo.Number.Should().NotBeNull().And.NotBeEmpty();
            reservationInfo.Status.Should().Be(ReservationStatus.Tentative);
        }


        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldCreate_ReservationHistory()
        {
            var createdReservationId = await Fixture.CommandResponse.GetResultFromJson<Guid>();

            // prepare query
            var query = $"{createdReservationId}/history";

            //send query
            var queryResponse = await Fixture.Get(query);
            queryResponse.EnsureSuccessStatusCode();

            var reservationPagedList = await queryResponse.GetResultFromJson<PagedListResponse<ReservationHistory>>();

            reservationPagedList.Should().NotBeNull();
            reservationPagedList.Items.Should().NotBeNull();

            reservationPagedList.Items.Should().HaveCount(1);
            reservationPagedList.TotalItemCount.Should().Be(1);
            reservationPagedList.HasNextPage.Should().Be(false);

            var reservationInfo = reservationPagedList.Items.Single();

            reservationInfo.ReservationId.Should().Be(createdReservationId);
            reservationInfo.Description.Should().StartWith("Created tentative reservation with number");
        }
    }
}
