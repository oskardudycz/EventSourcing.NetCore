using FluentAssertions;
using Xunit;

namespace MediatR.Tests.Publishing;

public class NoHandlers
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

    [Fact]
    public async void GivenNonRegisteredQueryHandler_WhenPublishMethodIsBeingCalled_ThenThrowsAnError()
    {
        var ex = await Record.ExceptionAsync(async () =>
        {
            //Given
            var serviceLocator = new ServiceLocator();
            var mediator = new Mediator(type => serviceLocator.Get(type).Single());

            var @event = new IssueCreated("cleaning");

            //When
            await mediator.Publish(@event);
        });

        //Then
        ex.Should().NotBeNull();
    }
}
