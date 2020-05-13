using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Temperature.TemperatureMeasurements;
using SmartHome.Temperature.TemperatureMeasurements.Commands;
using SmartHome.Temperature.TemperatureMeasurements.Queries;

namespace SmartHome.Api
{
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
            Guard.Against.Null(commandBus, nameof(commandBus));
            Guard.Against.Null(queryBus, nameof(queryBus));
            Guard.Against.Null(idGenerator, nameof(idGenerator));

            this.commandBus = commandBus;
            this.queryBus = queryBus;
            this.idGenerator = idGenerator;
        }

        [HttpGet]
        public Task<IReadOnlyList<TemperatureMeasurement>> Get()
        {
            return queryBus.Send<GetTemperatureMeasurements, IReadOnlyList<TemperatureMeasurement>>(GetTemperatureMeasurements.Create());
        }

        [HttpPost]
        public async Task<IActionResult> Start()
        {
            var measurementId = idGenerator.New();

            Guard.Against.Default(measurementId, nameof(measurementId));

            var command = StartTemperatureMeasurement.Create(
                measurementId
            );

            await commandBus.Send(command);

            return Created("api/TemperatureMeasurements", measurementId);
        }


        [HttpPost("{id}/temperatures")]
        public async Task<IActionResult> ChangeSeat(Guid id, [FromBody] decimal temperature)
        {
            var command = RecordTemperature.Create(
                id,
                temperature
            );

            await commandBus.Send(command);

            return Ok();
        }
    }
}
