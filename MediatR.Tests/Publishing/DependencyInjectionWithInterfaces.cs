using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Publishing
{
    public class DependencyInjectionWithInterfaces
    {
        public class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList()
            {
                Tasks = new List<string>();
            }
        }

        public interface ITaskWasAdded: INotification
        {
            string TaskName { get; }
        }

        public class TaskWasAdded: ITaskWasAdded
        {
            public string TaskName { get; }

            public TaskWasAdded(string taskName)
            {
                TaskName = taskName;
            }
        }

        public class TaskWasAddedHandler: INotificationHandler<ITaskWasAdded>
        {
            private readonly TasksList _taskList;

            public TaskWasAddedHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public Task Handle(ITaskWasAdded @event, CancellationToken cancellationToken = default(CancellationToken))
            {
                _taskList.Tasks.Add(@event.TaskName);
                return Task.CompletedTask;
            }
        }

        public class OtherTaskWasAddedHandler: INotificationHandler<ITaskWasAdded>
        {
            private readonly Action _incrementCounter;

            public OtherTaskWasAddedHandler(Action incrementCounter)
            {
                _incrementCounter = incrementCounter;
            }

            public Task Handle(ITaskWasAdded @event, CancellationToken cancellationToken = default(CancellationToken))
            {
                _incrementCounter();
                return Task.CompletedTask;
            }
        }

        private readonly TasksList _taskList = new TasksList();
        private int _taskCounter = 0;
        private readonly ServiceCollection services = new ServiceCollection();

        public DependencyInjectionWithInterfaces()
        {
            services.AddSingleton(sp => _taskList);

            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<ServiceFactory>(sp => t => sp.GetService(t));

            services.AddScoped<TaskWasAddedHandler>();
            services.AddScoped<INotificationHandler<TaskWasAdded>>(sp => sp.GetService<TaskWasAddedHandler>());

            services.AddScoped(sp => new OtherTaskWasAddedHandler(() => _taskCounter++));
            services.AddScoped<INotificationHandler<TaskWasAdded>>(sp => sp.GetService<OtherTaskWasAddedHandler>());
        }

        [Fact]
        public async void GivenTwoHandlersForEventInterface_WhenPublishMethodIsBeingCalled_ThenTwoHandlersAreBeingCalled()
        {
            using (var sp = services.BuildServiceProvider())
            {
                var mediator = sp.GetService<IMediator>();

                //Given
                var @event = new TaskWasAdded("cleaning");

                //When
                await mediator.Publish(@event);

                //Then
                _taskList.Tasks.Count.Should().Be.EqualTo(1);
                _taskList.Tasks.Should().Have.SameValuesAs("cleaning", "cleaning");

                _taskCounter.Should().Be.EqualTo(1);
            }
        }
    }
}
