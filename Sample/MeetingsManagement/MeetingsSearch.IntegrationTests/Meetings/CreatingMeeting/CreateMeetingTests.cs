using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Testing;
using FluentAssertions;
using MeetingsSearch.Api;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.CreatingMeeting;
using Xunit;

namespace MeetingsSearch.IntegrationTests.Meetings.CreatingMeeting
{
    public class CreateMeetingFixture: ApiFixture<Startup>
    {
        protected override string ApiUrl => MeetingsSearchApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = $"Event Sourcing Workshop {DateTime.Now.Ticks}";

        public override async Task InitializeAsync()
        {
            // prepare event
            var @event = new MeetingCreated(
                MeetingId,
                MeetingName
            );

            // send meeting created event to internal event bus
            await Sut.PublishInternalEvent(@event);
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
        [Trait("Category", "Acceptance")]
        public async Task MeetingCreated_ShouldUpdateReadModel()
        {
            //send query
            var queryResponse = await fixture.Get(
                $"?filter={fixture.MeetingName}",
                maxNumberOfRetries: 10,
                retryIntervalInMs: 1000,
                check: async response =>
                    response.IsSuccessStatusCode
                    && (await response.Content.ReadAsStringAsync()).Contains(fixture.MeetingName));

            queryResponse.EnsureSuccessStatusCode();

            var meetings = await queryResponse.GetResultFromJson<IReadOnlyCollection<Meeting>>();
            meetings.Should().Contain(meeting =>
                meeting.Id == fixture.MeetingId
                && meeting.Name == fixture.MeetingName
            );
        }
    }
}
