using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading.Tasks;

namespace MediatR.Tests.Sending
{
    public class SynchronousHandler
    {
        class ServiceLocator
        {
            private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

            public void Register(Type type, params object[] implementations)
                => Services.Add(type, implementations.ToList());

            public List<object> Get(Type type) { return Services[type]; }
        }

        public class TasksList
        {
            public List<string> Tasks { get; }

            public TasksList(params string[] tasks)
            {
                Tasks = tasks.ToList();
            }
        }

        public class GetTaskNamesQuery : IRequest<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        public class GetTaskNamesQueryHandler : IRequestHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly TasksList _taskList;
            public GetTaskNamesQueryHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public List<string> Handle(GetTaskNamesQuery query)
            {
                return _taskList.Tasks
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList();
            }
        }

        private IMediator mediator;

        public SynchronousHandler()
        {
            var queryHandler = new GetTaskNamesQueryHandler(
                new TasksList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IPipelineBehavior<GetTaskNamesQuery, List<string>>), new object[] { });
            serviceLocator.Register(typeof(IRequestHandler<GetTaskNamesQuery, List<string>>), queryHandler);

            mediator = new Mediator(
                    type => serviceLocator.Get(type).FirstOrDefault(),
                    type => serviceLocator.Get(type));
        }

        [Fact]
        public async void GivenRegisteredSynchronousRequestHandler_WhenSendMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var query = new GetTaskNamesQuery("cleaning");

            //When
            var result = await mediator.Send(query);

            //Then
            result.Should().Have.Count.EqualTo(2);
            result.Should().Have.SameValuesAs("Cleaning main room", "cleaning kitchen");
        }
    }
}
