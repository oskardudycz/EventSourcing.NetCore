using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Shipments.Api.Tests.Core;
using Tickets.Api.Requests;
using Tickets.Api.Responses;
using Tickets.Api.Tests.Config;
using Tickets.Reservations;
using Tickets.Reservations.Events;
using Tickets.Reservations.Projections;
using Xunit;

namespace Tickets.Api.Tests.Reservations
{
    public class CreateTentativeReservationFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl { get; } = "/api/Reservations";

        protected override Dictionary<string, string> GetConfiguration(string fixtureName) =>
            TestConfiguration.Get(fixtureName);

        public readonly Guid SeatId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse;

        public override async Task InitializeAsync()
        {
            // send create command
            CommandResponse = await PostAsync(new CreateTentativeReservationRequest {SeatId = SeatId});
        }
    }

    public class CreateTentativeReservationTests: IClassFixture<CreateTentativeReservationFixture>
    {
        private readonly CreateTentativeReservationFixture fixture;

        public CreateTentativeReservationTests(CreateTentativeReservationFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_ReservationId()
        {
            var commandResponse = fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().NotBeNull();

            var createdId = commandResult.FromJson<Guid>();
            createdId.Should().NotBeEmpty();
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldPublish_TentativeReservationCreated()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            fixture.PublishedInternalEventsOfType<TentativeReservationCreated>()
                .Should()
                .HaveCount(1)
                .And.Contain(@event =>
                    @event.ReservationId == createdReservationId
                    && @event.SeatId == fixture.SeatId
                    && !string.IsNullOrEmpty(@event.Number)
                );
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldCreate_ReservationDetailsReadModel()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            // prepare query
            var query = $"{createdReservationId}";

            //send query
            var queryResponse = await fixture.GetAsync(query);
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var reservationDetails = queryResult.FromJson<ReservationDetails>();
            reservationDetails.Id.Should().Be(createdReservationId);
            reservationDetails.Number.Should().NotBeNull().And.NotBeEmpty();
            reservationDetails.Status.Should().Be(ReservationStatus.Tentative);
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldCreate_ReservationList()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            //send query
            var queryResponse = await fixture.GetAsync();
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var reservationPagedList = queryResult.FromJson<PagedListResponse<ReservationShortInfo>>();

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
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldCreate_ReservationHistory()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            // prepare query
            var query = $"{createdReservationId}/history";

            //send query
            var queryResponse = await fixture.GetAsync(query);
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var reservationPagedList = queryResult.FromJson<PagedListResponse<ReservationHistory>>();

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
