using System;
using Marten.Events.Aggregation;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;

namespace MeetingsManagement.Meetings.GettingMeeting
{
    public class MeetingView
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = default!;

        public DateTime Created { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }
    }

    public class MeetingViewProjection: AggregateProjection<MeetingView>
    {
        public void Apply(MeetingCreated @event, MeetingView view)
        {
            view.Id = @event.MeetingId;
            view.Name = @event.Name;
            view.Created = @event.Created;
        }

        public void Apply(MeetingScheduled @event, MeetingView view)
        {
            view.Id = @event.MeetingId;
            view.Start = @event.Occurs.Start;
            view.End = @event.Occurs.End;
        }
    }
}
