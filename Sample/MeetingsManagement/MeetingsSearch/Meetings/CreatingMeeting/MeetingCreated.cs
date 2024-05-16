using Core.ElasticSearch.Repository;
using Core.Events;

namespace MeetingsSearch.Meetings.CreatingMeeting;

internal class MeetingCreated(Guid meetingId, string name)
{
    public Guid MeetingId { get; } = meetingId;
    public string Name { get; } = name;
}

internal class HandleMeetingCreated(IElasticSearchRepository<Meeting> repository): IEventHandler<MeetingCreated>
{
    public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
    {
        var meeting = new Meeting(@event.MeetingId, @event.Name);

        return repository.Add(@event.MeetingId, meeting, cancellationToken);
    }
}
