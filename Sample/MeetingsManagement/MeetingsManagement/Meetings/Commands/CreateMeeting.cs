using System;
using Core.Commands;

namespace MeetingsManagement.Meetings.Commands
{
    public class CreateMeeting: ICommand
    {
        public Guid Id { get; }
        public string Name { get; }

        public CreateMeeting(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
