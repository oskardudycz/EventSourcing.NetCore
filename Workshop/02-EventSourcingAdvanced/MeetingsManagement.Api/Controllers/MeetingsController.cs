using System;
using System.Threading.Tasks;
using Core.Commands;
using Core.Queries;
using MeetingsManagement.Meetings.Commands;
using MeetingsManagement.Meetings.Queries;
using MeetingsManagement.Meetings.ValueObjects;
using MeetingsManagement.Meetings.Views;
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
        public Task<MeetingView> Get(Guid id)
        {
            return _queryBus.Send<GetMeeting, MeetingView>(new GetMeeting(id));
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]CreateMeeting command)
        {
            await _commandBus.Send(command);

            return Created("api/Meetings", command.Id);
        }

        [HttpPost("{id}/schedule")]
        public async Task<IActionResult> Post(Guid id, [FromBody]Range occurs)
        {
            var command = new ScheduleMeeting(id, occurs);
            await _commandBus.Send(command);

            return Ok();
        }
    }
}
