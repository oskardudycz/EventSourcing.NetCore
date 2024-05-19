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
public class MotionSensorsController(
    ICommandBus commandBus,
    IQueryBus queryBus,
    IIdGenerator idGenerator)
    : Controller
{
    [HttpGet]
    public Task<IReadOnlyList<MotionSensor>> Get() =>
        queryBus.Query<GetMotionSensors, IReadOnlyList<MotionSensor>>(GetMotionSensors.Create());

    [HttpPost]
    public async Task<IActionResult> Start()
    {
        var measurementId = idGenerator.New();

        var command = InstallMotionSensor.Create(
            measurementId
        );

        await commandBus.Send(command);

        return Created($"/api/MotionSensors/{measurementId}", measurementId);
    }

    [HttpPost("rebuild")]
    public async Task<IActionResult> Rebuild()
    {
        await commandBus.Send(new RebuildMotionSensorsViews());

        return NoContent();
    }
}
