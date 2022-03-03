using Core.ElasticSearch.Repository;
using Core.Events;

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
    private readonly IElasticSearchRepository<Meeting> repository;

    public HandleMeetingCreated(IElasticSearchRepository<Meeting> repository)
    {
        this.repository = repository;
    }

    public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
    {
        var meeting = new Meeting(@event.MeetingId, @event.Name);

        return repository.Add(meeting, cancellationToken);
    }
}
