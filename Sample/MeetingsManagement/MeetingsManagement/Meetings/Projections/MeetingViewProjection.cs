using Marten.Events.Aggregation;
using MeetingsManagement.Meetings.Events;
using MeetingsManagement.Meetings.Views;

namespace MeetingsManagement.Meetings.Projections
{
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
