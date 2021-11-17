using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.Repositories;

namespace MeetingsSearch.Meetings.CreatingMeeting;

internal class MeetingCreated: IEvent
{
    public Guid MeetingId { get; }
    public string Name { get; }

    public MeetingCreated(Guid meetingId, string name)
    {
        MeetingId = meetingId;
        Name = name;
    }
}

internal class HandleMeetingCreated: IEventHandler<MeetingCreated>
{
    private readonly IRepository<Meeting> repository;

    public HandleMeetingCreated(IRepository<Meeting> repository)
    {
        this.repository = repository;
    }

    public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
    {
        var meeting = new Meeting(@event.MeetingId, @event.Name);

        return repository.Add(meeting, cancellationToken);
    }
}