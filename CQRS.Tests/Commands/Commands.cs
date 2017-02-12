using CQRS.Tests.TestsInfrasructure;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace CQRS.Tests.Commands
{
    public class Commands
    {
        interface ICommand : IRequest { }

        interface ICommandHandler<T> : IRequestHandler<T> where T: ICommand { }

        interface ICommandBus
        {
            Task Send(ICommand command);
        }

        class CommandBus : ICommandBus
        {
            private IMediator _mediator;

            internal CommandBus(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task Send(ICommand command)
            {
                return _mediator.Send(command);
            }
        }

        class AddTaskCommand : ICommand
        {
            public string Name { get; }

            internal AddTaskCommand(string name)
            {
                Name = name;
            }
        }

        interface ITaskApplicationService
        {
            Task AddTask(AddTaskCommand command);
        }

        class TaskApplicationService : ITaskApplicationService
        {
            private ICommandBus _commandBus;

            internal TaskApplicationService(ICommandBus commandBus)
            {
                _commandBus = commandBus;
            }

            public Task AddTask(AddTaskCommand command)
            {
                return _commandBus.Send(command);
            }
        }

        interface IAppWrtiteModel
        {
            IList<string> Tasks { get; }
        }

        class AppWriteModel : IAppWrtiteModel
        {
            public IList<string> Tasks { get; }

            public AppWriteModel(params string[] tasks)
            {
                Tasks = new List<string>();
            }
        }

        class AddTaskCommandHandler : ICommandHandler<AddTaskCommand>
        {
            private IAppWrtiteModel _writeModel;

            internal AddTaskCommandHandler(IAppWrtiteModel writeModel)
            {
                _writeModel = writeModel;
            }
            public void Handle(AddTaskCommand message)
            {
                _writeModel.Tasks.Add(message.Name);
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

            var taskName = "test";

            var command = new AddTaskCommand(taskName);

            applicationService.AddTask(command);

            writeModel.Tasks.Should().Have.Count.EqualTo(1);
            writeModel.Tasks.Should().Have.SameValuesAs(taskName);
        }
    }
}
