using Core.Commands;
using Core.Queries;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using MeetingsManagement.Meetings.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace MeetingsManagement.Api.Controllers;

[Route("api/[controller]")]
public class MeetingsController: Controller
{
    private readonly ICommandBus commandBus;
    private readonly IQueryBus queryBus;

    public MeetingsController(ICommandBus commandBus, IQueryBus queryBus)
    {
        this.commandBus = commandBus;
        this.queryBus = queryBus;
    }

    [HttpGet("{id}")]
    public Task<MeetingView> Get(Guid id)
    {
        return queryBus.Send<GetMeeting, MeetingView>(new GetMeeting(id));
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody]CreateMeeting command)
    {
        await commandBus.Send(command);

        return Created("api/Meetings", command.Id);
    }

    [HttpPost("{id}/schedule")]
    public async Task<IActionResult> Post(Guid id, [FromBody]DateRange occurs)
    {
        var command = new ScheduleMeeting(id, occurs);
        await commandBus.Send(command);

        return Ok();
    }
}