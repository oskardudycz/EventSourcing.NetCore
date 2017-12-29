using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Sending
{
    public class AsynchronousHandler
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

        private class GetTaskNamesQuery : IRequest<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        private class GetTaskNamesQueryAsyncHandler : IRequestHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly TasksList _taskList;

            public GetTaskNamesQueryAsyncHandler(TasksList tasksList)
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

        public AsynchronousHandler()
        {
            var queryHandler = new GetTaskNamesQueryAsyncHandler(
                new TasksList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IRequestHandler<GetTaskNamesQuery, List<string>>), queryHandler);
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IPipelineBehavior<GetTaskNamesQuery, List<string>>), new object[] { });

            mediator = new Mediator(
                    type => serviceLocator.Get(type).FirstOrDefault(),
                    type => serviceLocator.Get(type));
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