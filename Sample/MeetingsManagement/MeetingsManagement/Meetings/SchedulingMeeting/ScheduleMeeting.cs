using System;
using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using Core.Exceptions;
using Core.Marten.Repository;
using MediatR;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.SchedulingMeeting
{
    public record ScheduleMeeting(
        Guid MeetingId,
        DateRange Occurs
    ): ICommand;


    internal class HandleScheduleMeeting:
        ICommandHandler<ScheduleMeeting>
    {
        private readonly IMartenRepository<Meeting> repository;

        public HandleScheduleMeeting(
            IMartenRepository<Meeting> repository
        )
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<Unit> Handle(ScheduleMeeting command, CancellationToken cancellationToken)
        {
            var (meetingId, dateRange) = command;

            var meeting = await repository.Find(meetingId, cancellationToken)
                          ?? throw AggregateNotFoundException.For<Meeting>(meetingId);

            meeting.Schedule(dateRange);

            await repository.Update(meeting, cancellationToken);

            return Unit.Value;
        }
    }
}
