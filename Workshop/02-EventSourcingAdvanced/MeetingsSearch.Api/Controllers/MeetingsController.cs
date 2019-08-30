using System.Collections.Generic;
using System.Threading.Tasks;
using Core.Queries;
using MeetingsSearch.Meetings;
using MeetingsSearch.Meetings.Queries;
using Microsoft.AspNetCore.Mvc;

namespace MeetingsManagement.Api.Controllers
{
    [Route("api/[controller]")]
    public class MeetingsController: Controller
    {
        private readonly IQueryBus _queryBus;

        public MeetingsController(IQueryBus queryBus)
        {
            _queryBus = queryBus;
        }

        [HttpGet]
        public Task<IReadOnlyCollection<Meeting>> Search([FromQuery]string filter)
        {
            return _queryBus.Send<SearchMeetings, IReadOnlyCollection<Meeting>>(new SearchMeetings(filter));
        }
    }
}
