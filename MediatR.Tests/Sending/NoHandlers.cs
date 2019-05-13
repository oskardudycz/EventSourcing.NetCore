using System;
using System.Collections.Generic;
using System.Linq;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Publishing
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

            public TasksList(params string[] tasks)
            {
                Tasks = tasks.ToList();
            }
        }

        private class GetTaskNamesQuery: IRequest<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        [Fact]
        public async void GivenNonRegisteredQueryHandler_WhenSendMethodIsBeingCalled_ThenThrowsAnError()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                //Given
                var serviceLocator = new ServiceLocator();
                var mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());

                var query = new GetTaskNamesQuery("cleaning");

                //When
                var result = await mediator.Send(query);
            });

            //Then
            ex.Should().Not.Be.Null();
        }
    }
}
