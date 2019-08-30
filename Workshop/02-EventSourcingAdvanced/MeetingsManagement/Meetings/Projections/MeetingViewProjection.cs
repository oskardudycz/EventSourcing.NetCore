using System;
using Marten.Events.Projections;
using MeetingsManagement.Meetings.Events;
using MeetingsManagement.Meetings.Views;

namespace MeetingsManagement.Meetings.Projections
{
    public class MeetingViewProjection: ViewProjection<MeetingView, Guid>
    {
        public MeetingViewProjection()
        {
            ProjectEvent<MeetingCreated>(e => e.MeetingId, Apply);
            ProjectEvent<MeetingScheduled>(e => e.MeetingId, Apply);
        }

        private void Apply(MeetingView view, MeetingCreated @event)
        {
            view.Id = @event.MeetingId;
            view.Name = @event.Name;
            view.Created = @event.Created;
        }

        private void Apply(MeetingView view, MeetingScheduled @event)
        {
            view.Id = @event.MeetingId;
            view.Start = @event.Occurs.Start;
            view.End = @event.Occurs.End;
        }
    }
}
