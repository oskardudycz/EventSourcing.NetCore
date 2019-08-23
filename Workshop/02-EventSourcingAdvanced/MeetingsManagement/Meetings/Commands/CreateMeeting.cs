using Core.Commands;

namespace MeetingsManagement.Meetings.Commands
{
    public class CreateMeeting: ICommand
    {
        public string Name { get; }

        public CreateMeeting(string name)
        {
            Name = name;
        }
    }
}
