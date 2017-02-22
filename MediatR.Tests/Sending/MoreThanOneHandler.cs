using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace MediatR.Tests.Sending
{
    public class MoreThanOneHandler
    {
        class ServiceLocator
        {
            private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

            public void Register(Type type, params object[] implementations)
                => Services.Add(type, implementations.ToList());

            public List<object> Get(Type type) { return Services[type]; }
        }

        class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList(params string[] tasks)
            {
                Tasks = new List<string>();
            }
        }

        class AddTaskCommand : IRequest
        {
            public string TaskName { get; }

            public AddTaskCommand(string taskName)
            {
                TaskName = taskName;
            }
        }

        class AddTaskCommandHandler : IRequestHandler<AddTaskCommand>
        {
            private readonly TasksList _taskList;
            public AddTaskCommandHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public void Handle(AddTaskCommand command)
            {
                _taskList.Tasks.Add(command.TaskName);
            }
        }

        private readonly IMediator mediator;
        private readonly TasksList _taskList = new TasksList();

        public MoreThanOneHandler()
        {
            var commandHandler = new AddTaskCommandHandler(_taskList);

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IRequestHandler<AddTaskCommand>), commandHandler, commandHandler);
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IPipelineBehavior<AddTaskCommand, Unit>), new object[] { });
            serviceLocator.Register(typeof(IAsyncRequestHandler<AddTaskCommand>), new IAsyncRequestHandler<AddTaskCommand>[] { });


            mediator = new Mediator(
                    type => commandHandler,
                    type => serviceLocator.Get(type));
        }

        [Fact]
        public async void GivenTwoHandlersForOneCommand_WhenSendMethodIsBeingCalled_ThenOnlyFIrstHandlersIsBeingCalled()
        {
            //Given
            var query = new AddTaskCommand("cleaning");

            //When
            await mediator.Send(query);

            //Then
            _taskList.Tasks.Count.Should().Be.EqualTo(1);
            _taskList.Tasks.Should().Have.SameValuesAs("cleaning");
        }
    }
}
