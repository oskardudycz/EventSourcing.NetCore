using Core.Commands;
using Core.Ids;
using Core.Queries;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Temperature.TemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.GettingTemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Api.Controllers;

[Route("api/temperature-measurements")]
public class TemperatureMeasurementsController: Controller
{
    private readonly ICommandBus commandBus;
    private readonly IQueryBus queryBus;
    private readonly IIdGenerator idGenerator;

    public TemperatureMeasurementsController(
        ICommandBus commandBus,
        IQueryBus queryBus,
        IIdGenerator idGenerator)
    {
        this.commandBus = commandBus;
        this.queryBus = queryBus;
        this.idGenerator = idGenerator;
    }

    [HttpGet]
    public Task<IReadOnlyList<TemperatureMeasurement>> Get() =>
        queryBus.Send<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>(
            new GetTemperatureMeasurements()
        );

    [HttpPost]
    public async Task<IActionResult> Start()
    {
        var measurementId = idGenerator.New();

        var command = StartTemperatureMeasurement.Create(
            measurementId
        );

        await commandBus.Send(command);

        return Created($"/api/TemperatureMeasurements/{measurementId}", measurementId);
    }


    [HttpPost("{id}/temperatures")]
    public async Task<IActionResult> Record(Guid id, [FromBody] decimal temperature)
    {
        var command = RecordTemperature.Create(
            id,
            temperature
        );

        await commandBus.Send(command);

        return Ok();
    }
}
