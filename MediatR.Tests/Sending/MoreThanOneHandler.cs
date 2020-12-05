using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Sending
{
    public class MoreThanOneHandler
    {
        public class ServiceLocator
        {
            private readonly Dictionary<Type, List<object>> Services = new Dictionary<Type, List<object>>();

            public void Register(Type type, params object[] implementations)
                => Services.Add(type, implementations.ToList());

            public List<object> Get(Type type)
            {
                return Services[type];
            }
        }

        public class IssuesList
        {
            public List<string> Issues { get; }

            public IssuesList(params string[] issues)
            {
                Issues = issues?.ToList();
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
            private readonly IssuesList _issuesList;

            public CreateIssueCommandHandler(IssuesList issuesList)
            {
                _issuesList = issuesList;
            }

            public Task<Unit> Handle(CreateIssueCommand command, CancellationToken cancellationToken = default(CancellationToken))
            {
                _issuesList.Issues.Add(command.IssueName);
                return Unit.Task;
            }
        }

        private readonly IMediator mediator;
        private readonly IssuesList _issuesList = new IssuesList();

        public MoreThanOneHandler()
        {
            var commandHandler = new CreateIssueCommandHandler(_issuesList);

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IRequestHandler<CreateIssueCommand, Unit>), commandHandler, commandHandler);
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IEnumerable<IPipelineBehavior<CreateIssueCommand, Unit>>), new List<IPipelineBehavior<CreateIssueCommand, Unit>>());

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenTwoHandlersForOneCommand_WhenSendMethodIsBeingCalled_ThenOnlyFIrstHandlersIsBeingCalled()
        {
            //Given
            var query = new CreateIssueCommand("cleaning");

            //When
            await mediator.Send(query);

            //Then
            _issuesList.Issues.Count.Should().Be.EqualTo(1);
            _issuesList.Issues.Should().Have.SameValuesAs("cleaning");
        }
    }
}
