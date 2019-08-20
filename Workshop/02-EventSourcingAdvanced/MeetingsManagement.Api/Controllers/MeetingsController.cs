using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Core.Commands;
using Core.Queries;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;

namespace MeetingsManagement.Api.Controllers
{
    [Route("api/[controller]")]
    public class MeetingsController: Controller
    {
        private readonly ICommandBus _commandBus;
        private readonly IQueryBus _queryBus;

        public MeetingsController(ICommandBus commandBus, IQueryBus queryBus)
        {
            _commandBus = commandBus;
            _queryBus = queryBus;
        }

        // GET api/values
        [HttpGet]
        public Task<MeetingSummary> Get(Guid meetingId)
        {
            return _queryBus.Send<GetMeeting, MeetingSummary>(new GetMeeting());
        }

        // POST api/values
        [HttpPost]
        public Task Post([FromBody]CreateMeeting command)
        {
            return _commandBus.Send(command);
        }
    }
}
