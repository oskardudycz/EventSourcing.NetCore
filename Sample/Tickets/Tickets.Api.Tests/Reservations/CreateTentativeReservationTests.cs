using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Tickets.Api.Requests;
using Tickets.Api.Tests.Core;
using Tickets.Reservations.Events;
using Xunit;

namespace Tickets.Api.Tests.Reservations
{
    public class CreateTentativeReservationFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl { get; } = "/api/Reservations";

        public readonly Guid SeatId = Guid.NewGuid();

        public HttpResponseMessage CommandResponse;

        public override async Task InitializeAsync()
        {
            // send create command
            CommandResponse = await PostAsync(new CreateTentativeReservationRequest { SeatId = SeatId });
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
        public async Task CreateCommand_ShouldPublish_MeetingCreateEvent()
        {
            var createdReservationId = await fixture.CommandResponse.GetResultFromJSON<Guid>();

            fixture.PublishedEventsOfType<TentativeReservationCreated>()
               .Should().Contain(@event =>
                   @event.ReservationId == createdReservationId
                   && @event.SeatId == fixture.SeatId
                   && !string.IsNullOrEmpty(@event.Number)
               );
        }

        // [Fact]
        // [Trait("Category", "Exercise")]
        // public async Task CreateCommand_ShouldUpdateReadModel()
        // {
        //     // prepare query
        //     var query = new GetMeeting(fixture.MeetingId);
        //
        //     //send query
        //     var queryResponse = await fixture.Client.GetAsync($"{MeetingsManagementApi.MeetingsUrl}/{fixture.MeetingId}");
        //     queryResponse.EnsureSuccessStatusCode();
        //
        //     var queryResult = await queryResponse.Content.ReadAsStringAsync();
        //     queryResult.Should().NotBeNull();
        //
        //     var meetingSummary = queryResult.FromJson<MeetingView>();
        //     meetingSummary.Id.Should().Be(fixture.MeetingId);
        //     meetingSummary.Name.Should().Be(fixture.MeetingName);
        // }
    }
}
