using Core.Commands;
using Core.Ids;
using Core.Queries;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Temperature.MotionSensors;
using SmartHome.Temperature.MotionSensors.GettingMotionSensor;
using SmartHome.Temperature.MotionSensors.InstallingMotionSensor;
using SmartHome.Temperature.MotionSensors.RebuildingMotionSensorsViews;

namespace SmartHome.Api.Controllers;

[Route("api/motion-sensors")]
public class MotionSensorsController: Controller
{
    private readonly ICommandBus commandBus;
    private readonly IQueryBus queryBus;
    private readonly IIdGenerator idGenerator;

    public MotionSensorsController(
        ICommandBus commandBus,
        IQueryBus queryBus,
        IIdGenerator idGenerator)
    {
        this.commandBus = commandBus;
        this.queryBus = queryBus;
        this.idGenerator = idGenerator;
    }

    [HttpGet]
    public Task<IReadOnlyList<MotionSensor>> Get()
    {
        return queryBus.Send<GetMotionSensors, IReadOnlyList<MotionSensor>>(GetMotionSensors.Create());
    }

    [HttpPost]
    public async Task<IActionResult> Start()
    {
        var measurementId = idGenerator.New();

        var command = InstallMotionSensor.Create(
            measurementId
        );

        await commandBus.Send(command);

        return Created("api/MotionSensors", measurementId);
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild()
    {
        await commandBus.Send(new RebuildMotionSensorsViews());

        return NoContent();
    }
}
