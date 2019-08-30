using System.Collections.Generic;
using Core.Queries;

namespace MeetingsSearch.Meetings.Queries
{
    public class SearchMeetings: IQuery<IReadOnlyCollection<Meeting>>
    {
        public string Filter { get; }

        public SearchMeetings(string filter)
        {
            Filter = filter;
        }
    }
}
