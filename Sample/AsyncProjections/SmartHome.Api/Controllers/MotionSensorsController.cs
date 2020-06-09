using System.Collections.Generic;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Core.Commands;
using Core.Ids;
using Core.Queries;
using Microsoft.AspNetCore.Mvc;
using SmartHome.Temperature.MotionSensors;
using SmartHome.Temperature.MotionSensors.Commands;
using SmartHome.Temperature.MotionSensors.Queries;

namespace SmartHome.Api
{
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
            Guard.Against.Null(commandBus, nameof(commandBus));
            Guard.Against.Null(queryBus, nameof(queryBus));
            Guard.Against.Null(idGenerator, nameof(idGenerator));

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

            Guard.Against.Default(measurementId, nameof(measurementId));

            var command = InstallMotionSensor.Create(
                measurementId
            );

            await commandBus.Send(command);

            return Created("api/MotionSensors", measurementId);
        }

        [HttpPost("rebuild")]
        public async Task<IActionResult> Rebuild()
        {
            var command = RebuildMotionSensorsViews.Create();

            await commandBus.Send(command);

            return NoContent();
        }
    }
}
