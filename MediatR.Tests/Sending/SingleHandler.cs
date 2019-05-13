using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Sending
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

        private class GetTaskNamesQueryHandler: IRequestHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly TasksList _taskList;

            public GetTaskNamesQueryHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public Task<List<string>> Handle(GetTaskNamesQuery query, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.Run(() => _taskList.Tasks
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList());
            }
        }

        private readonly IMediator mediator;

        public SingleHandler()
        {
            var queryHandler = new GetTaskNamesQueryHandler(
                new TasksList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IRequestHandler<GetTaskNamesQuery, List<string>>), queryHandler);
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IEnumerable<IPipelineBehavior<GetTaskNamesQuery, List<string>>>), new List<IPipelineBehavior<GetTaskNamesQuery, List<string>>>());

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenRegisteredAsynchronousRequestHandler_WhenSendMethodIsBeingCalled_ThenReturnsProperResult()
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
