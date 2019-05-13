using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Tests.TestsInfrasructure;
using MediatR;
using SharpTestsEx;
using Xunit;

namespace CQRS.Tests.Queries
{
    public class Queries
    {
        private interface IQuery<out TResponse>: IRequest<TResponse>
        { }

        private interface IQueryHandler<in TQuery, TResponse>: IRequestHandler<TQuery, TResponse>
            where TQuery : IQuery<TResponse>
        { }

        private interface IQueryBus
        {
            Task<TResponse> Send<TResponse>(IQuery<TResponse> command);
        }

        private class QueryBus: IQueryBus
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

        private class GetTaskNamesQuery: IQuery<List<string>>
        {
            public string Filter { get; }

            public GetTaskNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        private interface ITaskApplicationService
        {
            Task<List<string>> GetTaskNames(GetTaskNamesQuery query);
        }

        private class TaskApplicationService: ITaskApplicationService
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

        private interface IAppReadModel
        {
            IQueryable<string> Tasks { get; }
        }

        private class AppReadModel: IAppReadModel
        {
            private readonly IList<string> _tasks;
            public IQueryable<string> Tasks => _tasks.AsQueryable();

            public AppReadModel(params string[] tasks)
            {
                _tasks = tasks?.ToList();
            }
        }

        private class AddTaskCommandHandler: IQueryHandler<GetTaskNamesQuery, List<string>>
        {
            private readonly IAppReadModel _readModel;

            internal AddTaskCommandHandler(IAppReadModel readModel)
            {
                _readModel = readModel;
            }

            public Task<List<string>> Handle(GetTaskNamesQuery query, CancellationToken cancellationToken = default(CancellationToken))
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
