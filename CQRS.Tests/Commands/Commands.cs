using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Tests.TestsInfrasructure;
using MediatR;
using SharpTestsEx;
using Xunit;

namespace CQRS.Tests.Commands
{
    public class Commands
    {
        private interface ICommand: IRequest
        { }

        private interface ICommandHandler<in T>: IRequestHandler<T> where T : ICommand
        { }

        private interface ICommandBus
        {
            Task Send(ICommand command);
        }

        private class CommandBus: ICommandBus
        {
            private readonly IMediator _mediator;

            internal CommandBus(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task Send(ICommand command)
            {
                return _mediator.Send(command);
            }
        }

        private class AddTaskCommand: ICommand
        {
            public string Name { get; }

            internal AddTaskCommand(string name)
            {
                Name = name;
            }
        }

        private interface ITaskApplicationService
        {
            Task AddTask(AddTaskCommand command);
        }

        private class TaskApplicationService: ITaskApplicationService
        {
            private readonly ICommandBus _commandBus;

            internal TaskApplicationService(ICommandBus commandBus)
            {
                _commandBus = commandBus;
            }

            public Task AddTask(AddTaskCommand command)
            {
                return _commandBus.Send(command);
            }
        }

        private interface IAppWrtiteModel
        {
            IList<string> Tasks { get; }
        }

        private class AppWriteModel: IAppWrtiteModel
        {
            public IList<string> Tasks { get; }

            public AppWriteModel(params string[] tasks)
            {
                Tasks = new List<string>();
            }
        }

        private class AddTaskCommandHandler: ICommandHandler<AddTaskCommand>
        {
            private readonly IAppWrtiteModel _writeModel;

            internal AddTaskCommandHandler(IAppWrtiteModel writeModel)
            {
                _writeModel = writeModel;
            }

            public Task<Unit> Handle(AddTaskCommand message, CancellationToken cancellationToken = default(CancellationToken))
            {
                _writeModel.Tasks.Add(message.Name);
                return Unit.Task;
            }
        }

        [Fact]
        public void GivenCommandWithData_WhenCommandIsSendToApplicationService_ThenWriteModelIsChanged()
        {
            var serviceLocator = new ServiceLocator();

            var writeModel = new AppWriteModel();
            var commandHandler = new AddTaskCommandHandler(writeModel);
            serviceLocator.RegisterCommandHandler<AddTaskCommand, AddTaskCommandHandler>(commandHandler);

            var applicationService = new TaskApplicationService(new CommandBus(serviceLocator.GetMediator()));

            //Given
            var addedTaskName = "cleaning";

            var command = new AddTaskCommand(addedTaskName);

            //When
            applicationService.AddTask(command);

            //Then
            writeModel.Tasks.Should().Have.Count.EqualTo(1);
            writeModel.Tasks.Should().Have.SameValuesAs(addedTaskName);
        }
    }
}
