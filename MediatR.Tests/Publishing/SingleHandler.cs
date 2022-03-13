using FluentAssertions;
using Xunit;

namespace MediatR.Tests.Publishing;

public class SingleHandler
{
    public class ServiceLocator
    {
        private readonly Dictionary<Type, List<object>> services = new Dictionary<Type, List<object>>();

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

        public IssuesList()
        {
            Issues = new List<string>();
        }
    }

    public class IssueCreated: INotification
    {
        public string IssueName { get; }

        public IssueCreated(string issueName)
        {
            IssueName = issueName;
        }
    }

    public class IssueCreatedHandler: INotificationHandler<IssueCreated>
    {
        private readonly IssuesList issuesList;

        public IssueCreatedHandler(IssuesList issuesList)
        {
            this.issuesList = issuesList;
        }

        public Task Handle(IssueCreated @event, CancellationToken cancellationToken = default)
        {
            issuesList.Issues.Add(@event.IssueName);
            return Task.CompletedTask;
        }
    }

    private readonly IMediator mediator;
    private readonly IssuesList issuesList = new IssuesList();

    public SingleHandler()
    {
        var notificationHandler = new IssueCreatedHandler(issuesList);

        var serviceLocator = new ServiceLocator();

        serviceLocator.Register(typeof(IEnumerable<INotificationHandler<IssueCreated>>),
            new object[] { new List<INotificationHandler<IssueCreated>> { notificationHandler } });

        mediator = new Mediator(type => serviceLocator.Get(type).Single());
    }

    [Fact]
    public async Task GivenRegisteredAsynchronousRequestHandler_WhenPublishMethodIsBeingCalled_ThenReturnsProperResult()
    {
        //Given
        var @event = new IssueCreated("cleaning");

        //When
        await mediator.Publish(@event);

        //Then
        issuesList.Issues.Should().HaveCount(1);
        issuesList.Issues.Should().BeEquivalentTo("cleaning");
    }
}
