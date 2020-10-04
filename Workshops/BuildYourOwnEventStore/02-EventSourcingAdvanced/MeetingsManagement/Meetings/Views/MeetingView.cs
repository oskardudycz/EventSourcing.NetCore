using System;

namespace MeetingsManagement.Meetings.Views
{
    public class MeetingView
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime Created { get; set; }

        public DateTime? Start { get; set; }

        public DateTime? End { get; set; }
    }
}
