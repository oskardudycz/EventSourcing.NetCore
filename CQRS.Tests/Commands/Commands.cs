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
        public interface ICommand: IRequest
        { }

        public interface ICommandHandler<in T>: IRequestHandler<T> where T : ICommand
        { }

        public interface ICommandBus
        {
            Task Send(ICommand command);
        }

        public class CommandBus: ICommandBus
        {
            private readonly IMediator _mediator;

            public CommandBus(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task Send(ICommand command)
            {
                return _mediator.Send(command);
            }
        }

        public class AddTaskCommand: ICommand
        {
            public string Name { get; }

            public AddTaskCommand(string name)
            {
                Name = name;
            }
        }

        public interface ITaskApplicationService
        {
            Task AddTask(AddTaskCommand command);
        }

        public class TaskApplicationService: ITaskApplicationService
        {
            private readonly ICommandBus _commandBus;

            public TaskApplicationService(ICommandBus commandBus)
            {
                _commandBus = commandBus;
            }

            public Task AddTask(AddTaskCommand command)
            {
                return _commandBus.Send(command);
            }
        }

        public interface IAppWrtiteModel
        {
            IList<string> Tasks { get; }
        }

        public class AppWriteModel: IAppWrtiteModel
        {
            public IList<string> Tasks { get; }

            public AppWriteModel(params string[] tasks)
            {
                Tasks = new List<string>();
            }
        }

        public class AddTaskCommandHandler: ICommandHandler<AddTaskCommand>
        {
            private readonly IAppWrtiteModel _writeModel;

            public AddTaskCommandHandler(IAppWrtiteModel writeModel)
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
