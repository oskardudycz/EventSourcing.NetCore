using FluentAssertions;
using Xunit;

namespace MediatR.Tests.Sending;

public class NoHandlers
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

    public class GetIssuesNamesQuery: IRequest<List<string>>
    {
        public string Filter { get; }

        public GetIssuesNamesQuery(string filter)
        {
            Filter = filter;
        }
    }

    [Fact]
    public async void GivenNonRegisteredQueryHandler_WhenSendMethodIsBeingCalled_ThenThrowsAnError()
    {
        var ex = await Record.ExceptionAsync(async () =>
        {
            //Given
            var serviceLocator = new ServiceLocator();
            var mediator = new Mediator(type => serviceLocator.Get(type).Single());

            var query = new GetIssuesNamesQuery("cleaning");

            //When
            var result = await mediator.Send(query);
        });

        //Then
        ex.Should().NotBeNull();
    }
}
