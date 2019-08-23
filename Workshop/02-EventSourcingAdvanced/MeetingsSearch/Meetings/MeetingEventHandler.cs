using System.Threading;
using System.Threading.Tasks;
using Core.Events;
using Core.Storage;
using MeetingsSearch.Meetings.Events;

namespace MeetingsSearch.Meetings
{
    internal class MeetingEventHandler: IEventHandler<MeetingCreated>
    {
        private readonly IRepository<Meeting> repository;

        public MeetingEventHandler(IRepository<Meeting> repository)
        {
            this.repository = repository;
        }

        public Task Handle(MeetingCreated @event, CancellationToken cancellationToken)
        {
            var meeting = new Meeting(@event.Id, @event.Name);

            return repository.Add(meeting, cancellationToken);
        }
    }
}
