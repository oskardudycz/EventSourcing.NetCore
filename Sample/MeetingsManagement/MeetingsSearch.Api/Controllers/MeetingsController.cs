using Core.Queries;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.SearchingMeetings;
using Microsoft.AspNetCore.Mvc;

namespace MeetingsSearch.Api.Controllers;

[Route("api/[controller]")]
public class MeetingsController(IQueryBus queryBus): Controller
{
    [HttpGet]
    public Task<IReadOnlyCollection<Meeting>> Search([FromQuery]string filter)
    {
        return queryBus.Query<SearchMeetings, IReadOnlyCollection<Meeting>>(new SearchMeetings(filter));
    }
}
