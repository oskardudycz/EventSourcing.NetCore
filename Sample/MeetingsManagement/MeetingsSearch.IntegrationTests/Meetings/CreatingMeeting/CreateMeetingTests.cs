using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Api.Testing;
using Core.Testing;
using FluentAssertions;
using MeetingsSearch.Api;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.CreatingMeeting;
using Xunit;
using Xunit.Abstractions;

namespace MeetingsSearch.IntegrationTests.Meetings.CreatingMeeting
{
    public class CreateMeetingFixture: ApiWithEventsFixture<Startup>
    {
        protected override string ApiUrl => MeetingsSearchApi.MeetingsUrl;

        public readonly Guid MeetingId = Guid.NewGuid();
        public readonly string MeetingName = $"Event Sourcing Workshop {DateTime.Now.Ticks}";

        protected override async Task Setup()
        {
            // prepare event
            var @event = new MeetingCreated(
                MeetingId,
                MeetingName
            );

            // send meeting created event to internal event bus
            await PublishInternalEvent(@event);
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
        public async Task MeetingCreated_ShouldUpdateReadModel()
        {
            //send query
            var queryResponse = await Fixture.Get(
                $"?filter={Fixture.MeetingName}",
                maxNumberOfRetries: 10,
                retryIntervalInMs: 1000,
                check: async response =>
                    response.IsSuccessStatusCode
                    && (await response.Content.ReadAsStringAsync()).Contains(Fixture.MeetingName));

            queryResponse.EnsureSuccessStatusCode();

            var meetings = await queryResponse.GetResultFromJson<IReadOnlyCollection<Meeting>>();
            meetings.Should().Contain(meeting =>
                meeting.Id == Fixture.MeetingId
                && meeting.Name == Fixture.MeetingName
            );
        }
    }
}
