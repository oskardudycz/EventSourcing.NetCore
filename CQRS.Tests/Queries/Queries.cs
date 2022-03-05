using CQRS.Tests.TestsInfrasructure;
using FluentAssertions;
using MediatR;
using Xunit;

namespace CQRS.Tests.Queries;

public class Queries
{
    public interface IQuery<out TResponse>: IRequest<TResponse>
    { }

    public interface IQueryHandler<in TQuery, TResponse>: IRequestHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    { }

    public interface IQueryBus
    {
        Task<TResponse> Send<TResponse>(IQuery<TResponse> command);
    }

    public class QueryBus: IQueryBus
    {
        private readonly IMediator mediator;

        public QueryBus(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Task<TResponse> Send<TResponse>(IQuery<TResponse> command)
        {
            return mediator.Send(command);
        }
    }

    public class GetIssuesNamesQuery: IQuery<List<string>>
    {
        public string Filter { get; }

        public GetIssuesNamesQuery(string filter)
        {
            Filter = filter;
        }
    }

    public interface IIssueApplicationService
    {
        Task<List<string>> GetIssuesNames(GetIssuesNamesQuery query);
    }

    public class IssueApplicationService: IIssueApplicationService
    {
        private readonly IQueryBus queryBus;

        public IssueApplicationService(IQueryBus queryBus)
        {
            this.queryBus = queryBus;
        }

        public Task<List<string>> GetIssuesNames(GetIssuesNamesQuery query)
        {
            return queryBus.Send(query);
        }
    }

    public interface IAppReadModel
    {
        IQueryable<string> Issues { get; }
    }

    public class AppReadModel: IAppReadModel
    {
        private readonly IList<string> issues;
        public IQueryable<string> Issues => issues.AsQueryable();

        public AppReadModel(params string[] issues)
        {
            this.issues = issues.ToList();
        }
    }

    public class CreateIssueCommandHandler: IQueryHandler<GetIssuesNamesQuery, List<string>>
    {
        private readonly IAppReadModel readModel;

        public CreateIssueCommandHandler(IAppReadModel readModel)
        {
            this.readModel = readModel;
        }

        public Task<List<string>> Handle(GetIssuesNamesQuery query, CancellationToken cancellationToken = default(CancellationToken))
        {
            return Task.Run(() => readModel.Issues
                .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                .ToList(), cancellationToken);
        }
    }

    [Fact]
    public async void GivenCommandWithData_WhenCommandIsSendToApplicationService_ThenreadModelIsChanged()
    {
        var serviceLocator = new ServiceLocator();

        var readModel = new AppReadModel("Cleaning main room", "Writing blog", "cleaning kitchen");
        var commandHandler = new CreateIssueCommandHandler(readModel);
        serviceLocator.RegisterQueryHandler(commandHandler);

        var applicationService = new IssueApplicationService(new QueryBus(serviceLocator.GetMediator()));

        //Given
        var query = new GetIssuesNamesQuery("cleaning");

        //When
        var result = await applicationService.GetIssuesNames(query);

        //Then
        result.Should().HaveCount(2);
        result.Should().BeEquivalentTo("Cleaning main room", "cleaning kitchen");
    }
}
