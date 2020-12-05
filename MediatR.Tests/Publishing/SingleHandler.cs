using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SharpTestsEx;
using Xunit;

namespace MediatR.Tests.Publishing
{
    public class SingleHandler
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
            private readonly IssuesList _issuesList;

            public IssueCreatedHandler(IssuesList issuesList)
            {
                _issuesList = issuesList;
            }

            public Task Handle(IssueCreated @event, CancellationToken cancellationToken = default)
            {
                _issuesList.Issues.Add(@event.IssueName);
                return Task.CompletedTask;
            }
        }

        private readonly IMediator mediator;
        private readonly IssuesList _issuesList = new IssuesList();

        public SingleHandler()
        {
            var notificationHandler = new IssueCreatedHandler(_issuesList);

            var serviceLocator = new ServiceLocator();

            serviceLocator.Register(typeof(IEnumerable<INotificationHandler<IssueCreated>>),
                new object[] { new List<INotificationHandler<IssueCreated>> { notificationHandler } });

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenRegisteredAsynchronousRequestHandler_WhenPublishMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var @event = new IssueCreated("cleaning");

            //When
            await mediator.Publish(@event);

            //Then
            _issuesList.Issues.Should().Have.Count.EqualTo(1);
            _issuesList.Issues.Should().Have.SameValuesAs("cleaning");
        }
    }
}
