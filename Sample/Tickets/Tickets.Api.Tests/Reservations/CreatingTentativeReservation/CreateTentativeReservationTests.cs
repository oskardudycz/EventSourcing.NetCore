using System.Net;
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

namespace Tickets.Api.Tests.Reservations.CreatingTentativeReservation;

public class CreateTentativeReservationFixture: ApiWithEventsFixture<Startup>
{
    protected override string ApiUrl => "/api/Reservations";

    protected override Dictionary<string, string> GetConfiguration(string fixtureName) =>
        TestConfiguration.Get(fixtureName);

    public readonly Guid SeatId = Guid.NewGuid();

    public HttpResponseMessage CommandResponse = default!;

    public override async Task InitializeAsync()
    {
        // send create command
        CommandResponse = await Post(new CreateTentativeReservationRequest { SeatId = SeatId });
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
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldReturn_CreatedStatus_With_ReservationId()
    {
        var commandResponse = fixture.CommandResponse;
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
        var createdReservationId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        await fixture.ShouldPublishInternalEventOfType<TentativeReservationCreated>(
            @event =>
                @event.ReservationId == createdReservationId
                && @event.SeatId == fixture.SeatId
                && !string.IsNullOrEmpty(@event.Number)
        );
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldCreate_ReservationDetailsReadModel()
    {
        var createdReservationId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        // prepare query
        var query = $"{createdReservationId}";

        //send query
        var queryResponse = await fixture.Get(query);
        queryResponse.EnsureSuccessStatusCode();

        var reservationDetails = await queryResponse.GetResultFromJson<ReservationDetails>();
        reservationDetails.Id.Should().Be(createdReservationId);
        reservationDetails.Number.Should().NotBeNull().And.NotBeEmpty();
        reservationDetails.Status.Should().Be(ReservationStatus.Tentative);
    }

    [Fact]
    [Trait("Category", "Acceptance")]
    public async Task CreateCommand_ShouldCreate_ReservationList()
    {
        var createdReservationId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        //send query
        var queryResponse = await fixture.Get();
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
        var createdReservationId = await fixture.CommandResponse.GetResultFromJson<Guid>();

        // prepare query
        var query = $"{createdReservationId}/history";

        //send query
        var queryResponse = await fixture.Get(query);
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
