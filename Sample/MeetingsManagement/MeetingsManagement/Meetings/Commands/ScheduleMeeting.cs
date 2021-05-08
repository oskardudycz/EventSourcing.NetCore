using System;
using Core.Commands;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Meetings.Commands
{
    public record ScheduleMeeting(
        Guid MeetingId,
        DateRange Occurs
    ): ICommand;
}
