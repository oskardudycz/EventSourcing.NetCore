using SharpTestsEx;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading.Tasks;

namespace MediatR.Tests.Publishing
{
    public class Publishing
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

        public Publishing()
        {
        }

        [Fact]
        public async void GivenNonRegisteredQueryHandler_WhenPublishMethodIsBeingCalled_ThenThrowsAnError()
        {
            var ex = await Record.ExceptionAsync(async () =>
            {
                //Given
                var serviceLocator = new ServiceLocator();
                var mediator = new Mediator(
                        type => serviceLocator.Get(type).FirstOrDefault(),
                        type => serviceLocator.Get(type));

                var query = new GetTaskNamesQuery("cleaning");

                //When
                var result = await mediator.Send(query);
            });

            //Then
            ex.Should().Not.Be.Null();
        }

        [Fact]
        public async void GivenRegisteredSynchronousQueryHandler_WhenPublishMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var queryHandler = new GetTaskNamesQueryHandler(
                new TasksList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IPipelineBehavior<GetTaskNamesQuery, List<string>>), new object[] { });
            serviceLocator.Register(typeof(IRequestHandler<GetTaskNamesQuery, List<string>>), queryHandler);

            var mediator = new Mediator(
                    type => serviceLocator.Get(type).FirstOrDefault(),
                    type => serviceLocator.Get(type));

            var query = new GetTaskNamesQuery("cleaning");

            //When
            var result = await mediator.Send(query);

            //Then
            result.Should().Have.Count.EqualTo(2);
            result.Should().Have.SameValuesAs("Cleaning main room", "cleaning kitchen");
        }

        public class GetTaskNamesQueryAsyncHandler : IAsyncRequestHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly TasksList _taskList;
            public GetTaskNamesQueryAsyncHandler(TasksList tasksList)
            {
                _taskList = tasksList;
            }

            public Task<List<string>> Handle(GetTaskNamesQuery query)
            {
                return Task.Run(() => _taskList.Tasks
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList());
            }
        }

        [Fact]
        public async void GivenRegisteredAsynchronousQueryHandler_WhenPublishMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var queryHandler = new GetTaskNamesQueryAsyncHandler(
                new TasksList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IPipelineBehavior<GetTaskNamesQuery, List<string>>), new object[] { });
            serviceLocator.Register(typeof(IAsyncRequestHandler<GetTaskNamesQuery, List<string>>), queryHandler);

            var mediator = new Mediator(
                    type => serviceLocator.Get(type).FirstOrDefault(),
                    type => serviceLocator.Get(type));

            var query = new GetTaskNamesQuery("cleaning");

            //When
            var result = await mediator.Send(query);

            //Then
            result.Should().Have.Count.EqualTo(2);
            result.Should().Have.SameValuesAs("Cleaning main room", "cleaning kitchen");
        }
    }
}
