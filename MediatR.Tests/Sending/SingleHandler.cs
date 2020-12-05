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
                Issues = issues.ToList();
            }
        }

        public class GetIssuesNamesQuery: IRequest<List<string>>
        {
            public string Filter { get; }

            public GetIssuesNamesQuery(string filter)
            {
                Filter = filter;
            }
        }

        public class GetIssuesNamesQueryHandler: IRequestHandler<GetIssuesNamesQuery, List<string>>
        {
            private readonly IssuesList _issuesList;

            public GetIssuesNamesQueryHandler(IssuesList issuesList)
            {
                _issuesList = issuesList;
            }

            public Task<List<string>> Handle(GetIssuesNamesQuery query, CancellationToken cancellationToken = default)
            {
                return Task.Run(() => _issuesList.Issues
                    .Where(taskName => taskName.ToLower().Contains(query.Filter.ToLower()))
                    .ToList(), cancellationToken);
            }
        }

        private readonly IMediator mediator;

        public SingleHandler()
        {
            var queryHandler = new GetIssuesNamesQueryHandler(
                new IssuesList("Cleaning main room", "Writing blog", "cleaning kitchen"));

            var serviceLocator = new ServiceLocator();
            serviceLocator.Register(typeof(IRequestHandler<GetIssuesNamesQuery, List<string>>), queryHandler);
            //Registration needed internally by MediatR
            serviceLocator.Register(typeof(IEnumerable<IPipelineBehavior<GetIssuesNamesQuery, List<string>>>), new List<IPipelineBehavior<GetIssuesNamesQuery, List<string>>>());

            mediator = new Mediator(type => serviceLocator.Get(type).FirstOrDefault());
        }

        [Fact]
        public async void GivenRegisteredAsynchronousRequestHandler_WhenSendMethodIsBeingCalled_ThenReturnsProperResult()
        {
            //Given
            var query = new GetIssuesNamesQuery("cleaning");

            //When
            var result = await mediator.Send(query);

            //Then
            result.Should().Have.Count.EqualTo(2);
            result.Should().Have.SameValuesAs("Cleaning main room", "cleaning kitchen");
        }
    }
}
