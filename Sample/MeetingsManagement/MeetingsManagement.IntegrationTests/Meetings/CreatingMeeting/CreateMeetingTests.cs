using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using MeetingsManagement.Api;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using Xunit;
using Xunit.Abstractions;

namespace MeetingsManagement.IntegrationTests.Meetings.CreatingMeeting
{
    public class CreateMeetingFixture: ApiWithEventsFixture<Startup>
    {
        protected override string ApiUrl => MeetingsManagementApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = "Event Sourcing Workshop";

        public HttpResponseMessage CommandResponse = default!;

        protected override async Task Setup()
        {
            // prepare command
            var command = new CreateMeeting(
                MeetingId,
                MeetingName
            );

            // send create command
            CommandResponse = await Post(command);
        }
    }

    public class CreateMeetingTests: ApiTest<CreateMeetingFixture>
    {
        public CreateMeetingTests(CreateMeetingFixture fixture, ITestOutputHelper outputHelper)
            : base(fixture, outputHelper)
        {
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldReturn_CreatedStatus_With_MeetingId()
        {
            var commandResponse = Fixture.CommandResponse;
            commandResponse.EnsureSuccessStatusCode();
            commandResponse.StatusCode.Should().Be(HttpStatusCode.Created);

            // get created record id
            var createdId = await commandResponse.GetResultFromJson<Guid>();
            createdId.Should().Be(Fixture.MeetingId);
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public void CreateCommand_ShouldPublish_MeetingCreateEvent()
        {
            // assert MeetingCreated event was produced to external bus
            Fixture.PublishedExternalEventsOfType<MeetingCreated>()
               .Should().Contain(@event =>
                   @event.MeetingId == Fixture.MeetingId
                   && @event.Name == Fixture.MeetingName
               );
        }

        [Fact]
        [Trait("Category", "Acceptance")]
        public async Task CreateCommand_ShouldUpdateReadModel()
        {
            // prepare query
            var query = $"{Fixture.MeetingId}";

            //send query
            var queryResponse = await Fixture.Get(query);
            queryResponse.EnsureSuccessStatusCode();

            var meetingSummary =  await queryResponse.GetResultFromJson<MeetingView>();
            meetingSummary.Id.Should().Be(Fixture.MeetingId);
            meetingSummary.Name.Should().Be(Fixture.MeetingName);
        }
    }
}
