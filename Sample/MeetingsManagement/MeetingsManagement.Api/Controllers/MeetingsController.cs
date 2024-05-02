using Core.Commands;
using Core.Queries;
using Core.WebApi.Headers;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using MeetingsManagement.Meetings.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace MeetingsManagement.Api.Controllers;

[Route("api/[controller]")]
public class MeetingsController(ICommandBus commandBus, IQueryBus queryBus): Controller
{
    [HttpGet("{id}")]
    public async Task<MeetingView> Get(Guid id)
    {

        var result = await queryBus.Query<GetMeeting, MeetingView>(new GetMeeting(id));

        Response.TrySetETagResponseHeader(result.Version);

        return result;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]CreateMeeting command)
    {
        await commandBus.Send(command);

        return Created($"/api/Meetings/{command.Id}", command.Id);
    }

    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> Post(Guid id, [FromBody]DateRange occurs)
    {
        var command = new ScheduleMeeting(id, occurs);
        await commandBus.Send(command);

        return Ok();
    }
}
