using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using Meetings.IntegrationTests.MeetingsManagement;
using MeetingsManagement.Api;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Events;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.Views;
using Xunit;

namespace MeetingsManagement.IntegrationTests.Meetings
{
    public class CreateMeetingFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl { get; } = MeetingsManagementApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = "Event Sourcing Workshop";

        public HttpResponseMessage CommandResponse;

        public override async Task InitializeAsync()
        {
            // prepare command
            var command = new CreateMeeting(
                MeetingId,
                MeetingName
            );

            // send create command
            CommandResponse = await PostAsync(command);
        }
    }

    public class CreateMeetingTests: IClassFixture<CreateMeetingFixture>
    {
        private readonly CreateMeetingFixture fixture;

        public CreateMeetingTests(CreateMeetingFixture fixture)
        {
            this.fixture = fixture;
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_MeetingId()
        {
            var commandResponse = fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var commandResult = await commandResponse.Content.ReadAsStringAsync();
            commandResult.Should().NotBeNull();

            var createdId = commandResult.FromJson<Guid>();
            createdId.Should().Be(fixture.MeetingId);
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public void CreateCommand_ShouldPublish_MeetingCreateEvent()
        {
            // assert MeetingCreated event was produced to external bus
            fixture.PublishedExternalEventsOfType<MeetingCreated>()
               .Should().Contain(@event =>
                   @event.MeetingId == fixture.MeetingId
                   && @event.Name == fixture.MeetingName
               );
        }

        [Fact]
        [Trait("Category", "Exercise")]
        public async Task CreateCommand_ShouldUpdateReadModel()
        {
            // prepare query
            var query = $"{fixture.MeetingId}";

            //send query
            var queryResponse = await fixture.GetAsync(query);
            queryResponse.EnsureSuccessStatusCode();

            var queryResult = await queryResponse.Content.ReadAsStringAsync();
            queryResult.Should().NotBeNull();

            var meetingSummary = queryResult.FromJson<MeetingView>();
            meetingSummary.Id.Should().Be(fixture.MeetingId);
            meetingSummary.Name.Should().Be(fixture.MeetingName);
        }
    }
}
