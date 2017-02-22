using CQRS.Tests.TestsInfrasructure;
using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;
using System.Linq;

namespace CQRS.Tests.Queries
{
    public class Queries
    {
        interface IQuery<TResponse> : IRequest<TResponse> { }

        interface IQueryHandler<TQuery, TResponse> : IAsyncRequestHandler<TQuery, TResponse>
            where TQuery : IQuery<TResponse>
        { }

        interface IQueryBus
        {
            Task<TResponse> Send<TResponse>(IQuery<TResponse> command);
        }

        class QueryBus : IQueryBus
        {
            private readonly IMediator _mediator;

            internal QueryBus(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task<TResponse> Send<TResponse>(IQuery<TResponse> command)
            {
                return _mediator.Send(command);
            }
        }

        class GetTaskNamesQuery : IQuery<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        interface ITaskApplicationService
        {
            Task<List<string>> GetTaskNames(GetTaskNamesQuery query);
        }

        class TaskApplicationService : ITaskApplicationService
        {
            private readonly IQueryBus _queryBus;

            internal TaskApplicationService(IQueryBus queryBus)
            {
                _queryBus = queryBus;
            }

            public Task<List<string>> GetTaskNames(GetTaskNamesQuery query)
            {
                return _queryBus.Send(query);
            }
        }

        interface IAppReadModel
        {
            IQueryable<string> Tasks { get; }
        }

        class AppReadModel : IAppReadModel
        {
            private readonly IList<string> _tasks;
            public IQueryable<string> Tasks => _tasks.AsQueryable();

            public AppReadModel(params string[] tasks)
            {
                _tasks = tasks?.ToList();
            }
        }

        class AddTaskCommandHandler : IQueryHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly IAppReadModel _readModel;

            internal AddTaskCommandHandler(IAppReadModel readModel)
            {
                _readModel = readModel;
            }

            public Task<List<string>> Handle(GetTaskNamesQuery query)
            {
                return Task.Run(() => _readModel.Tasks
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList());
            }
        }

        [Fact]
        public async void GivenCommandWithData_WhenCommandIsSendToApplicationService_ThenreadModelIsChanged()
        {
            var serviceLocator = new ServiceLocator();

            var readModel = new AppReadModel("Cleaning main room", "Writing blog", "cleaning kitchen");
            var commandHandler = new AddTaskCommandHandler(readModel);
            serviceLocator.RegisterQueryHandler(commandHandler);

            var applicationService = new TaskApplicationService(new QueryBus(serviceLocator.GetMediator()));

            //Given
            var query = new GetTaskNamesQuery("cleaning");

            //When
            var result = await applicationService.GetTaskNames(query);

            //Then
            result.Should().Have.Count.EqualTo(2);
            result.Should().Have.SameValuesAs("Cleaning main room", "cleaning kitchen");
        }
    }
}
