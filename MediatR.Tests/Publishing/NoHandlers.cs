using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Sending
{
    public class NoHandlers
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

        [Fact]
        public async void GivenNonRegisteredQueryHandler_WhenPublishMethodIsBeingCalled_ThenThrowsAnError()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                //Given
                var serviceLocator = new ServiceLocator();
                var mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());

                var @event = new TaskWasAdded("cleaning");

                //When
                await mediator.Publish(@event);
            });

            //Then
            ex.Should().Not.Be.Null();
        }
    }
}
