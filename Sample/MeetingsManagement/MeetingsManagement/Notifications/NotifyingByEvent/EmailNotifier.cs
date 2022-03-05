using Core.Events;
using MeetingsManagement.Meetings.CreatingMeeting;

namespace MeetingsManagement.Notifications.NotifyingByEvent;

public class EmailNotifier: IEventHandler<MeetingCreated>
{
    public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
    {
        //some dummy logic, but here could be email sender placed
        Console.Write($"{@event.Name} has been created");

        return Task.CompletedTask;
    }
}
