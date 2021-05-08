using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using MeetingsManagement.Meetings.Events;

namespace MeetingsManagement.Notifications
{
    public class EmailNotifier: IEventHandler<MeetingCreated>
    {
        public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
        {
            //some dummy logic, but here could be email sender placed
            Console.Write($"{@event.Name} has been created");

            return Task.CompletedTask;
        }
    }
}
