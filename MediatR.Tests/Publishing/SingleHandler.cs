using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Publishing
{
    public class SingleHandler
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

        private class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList()
            {
                Tasks = new List<string>();
            }
        }

        private class TaskWasAdded: INotification
        {
            public string TaskName { get; }

            public TaskWasAdded(string taskName)
            {
                TaskName = taskName;
            }
        }

        private class TaskWasAddedHandler: INotificationHandler<TaskWasAdded>
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
        private readonly TasksList _tasksList = new TasksList();

        public SingleHandler()
        {
            var notificationHandler = new TaskWasAddedHandler(_tasksList);

            var serviceLocator = new ServiceLocator();

            serviceLocator.Register(typeof(IEnumerable<INotificationHandler<TaskWasAdded>>),
                new object[] { new List<INotificationHandler<TaskWasAdded>> { notificationHandler } });

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenRegisteredAsynchronousRequestHandler_WhenPublishMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var @event = new TaskWasAdded("cleaning");

            //When
            await mediator.Publish(@event);

            //Then
            _tasksList.Tasks.Should().Have.Count.EqualTo(1);
            _tasksList.Tasks.Should().Have.SameValuesAs("cleaning");
        }
    }
}
