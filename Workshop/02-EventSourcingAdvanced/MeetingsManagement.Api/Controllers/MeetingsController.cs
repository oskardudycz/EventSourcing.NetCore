using System;
using System.Threading.Tasks;
using Core.Commands;
using Core.Queries;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet("{id}")]
        public Task<MeetingSummary> Get(Guid id)
        {
            return _queryBus.Send<GetMeeting, MeetingSummary>(new GetMeeting(id));
        }

        [HttpPost]
        public Task Post([FromBody]CreateMeeting command)
        {
            return _commandBus.Send(command);
        }
    }
}
