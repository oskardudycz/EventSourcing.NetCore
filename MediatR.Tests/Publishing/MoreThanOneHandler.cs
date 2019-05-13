using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Publishing
{
    public class MoreThanOneHandler
    {
        private class ServiceLocator
        {
            private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

            public void Register(Type type, params object[] implementations)
                => Services.Add(type, implementations.ToList());

            public List<object> Get(Type type)
            {
                return Services[type];
            }
        }

        public class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList()
            {
                Tasks = new List<string>();
            }
        }

        public class TaskWasAdded: INotification
        {
            public string TaskName { get; }

            public TaskWasAdded(string taskName)
            {
                TaskName = taskName;
            }
        }

        public class TaskWasAddedHandler: INotificationHandler<TaskWasAdded>
        {
            private readonly TasksList _taskList;

            public TaskWasAddedHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public Task Handle(TaskWasAdded @event, CancellationToken cancellationToken = default(CancellationToken))
            {
                _taskList.Tasks.Add(@event.TaskName);
                return Task.CompletedTask;
            }
        }

        private readonly IMediator mediator;
        private readonly TasksList _taskList = new TasksList();

        public MoreThanOneHandler()
        {
            var eventHandler = new TaskWasAddedHandler(_taskList);

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IEnumerable<INotificationHandler<TaskWasAdded>>),
                new object[] { new List<INotificationHandler<TaskWasAdded>> { eventHandler, eventHandler } });

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenTwoHandlersForOneEvent_WhenPublishMethodIsBeingCalled_ThenTwoHandlersAreBeingCalled()
        {
            //Given
            var @event = new TaskWasAdded("cleaning");

            //When
            await mediator.Publish(@event);

            //Then
            _taskList.Tasks.Count.Should().Be.EqualTo(2);
            _taskList.Tasks.Should().Have.SameValuesAs("cleaning", "cleaning");
        }
    }
}
