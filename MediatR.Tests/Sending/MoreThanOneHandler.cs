using FluentAssertions;
using Xunit;

namespace MediatR.Tests.Sending;

public class MoreThanOneHandler
{
    public class ServiceLocator
    {
        private readonly Dictionary<Type, List<object>> services = new();

        public void Register(Type type, params object[] implementations)
            => services.Add(type, implementations.ToList());

        public List<object> Get(Type type)
        {
            return services[type];
        }
    }

    public class IssuesList
    {
        public List<string> Issues { get; }

        public IssuesList(params string[] issues)
        {
            Issues = issues.ToList();
        }
    }

    public class CreateIssueCommand: IRequest
    {
        public string IssueName { get; }

        public CreateIssueCommand(string issueName)
        {
            IssueName = issueName;
        }
    }

    public class CreateIssueCommandHandler: IRequestHandler<CreateIssueCommand>
    {
        private readonly IssuesList issuesList;

        public CreateIssueCommandHandler(IssuesList issuesList)
        {
            this.issuesList = issuesList;
        }

        public Task<Unit> Handle(CreateIssueCommand command, CancellationToken cancellationToken = default(CancellationToken))
        {
            issuesList.Issues.Add(command.IssueName);
            return Unit.Task;
        }
    }

    private readonly IMediator mediator;
    private readonly IssuesList issuesList = new IssuesList();

    public MoreThanOneHandler()
    {
        var commandHandler = new CreateIssueCommandHandler(issuesList);

        var serviceLocator = new ServiceLocator();
        serviceLocator.Register(typeof(IRequestHandler<CreateIssueCommand, Unit>), commandHandler, commandHandler);
        //Registration needed internally by MediatR
        serviceLocator.Register(typeof(IEnumerable<IPipelineBehavior<CreateIssueCommand, Unit>>), new List<IPipelineBehavior<CreateIssueCommand, Unit>>());

        mediator = new Mediator(type => serviceLocator.Get(type).First());
    }

    [Fact]
    public async void GivenTwoHandlersForOneCommand_WhenSendMethodIsBeingCalled_ThenOnlyFIrstHandlersIsBeingCalled()
    {
        //Given
        var query = new CreateIssueCommand("cleaning");

        //When
        await mediator.Send(query);

        //Then
        issuesList.Issues.Count.Should().Be(1);
        issuesList.Issues.Should().BeEquivalentTo("cleaning");
    }
}
