using CQRS.Tests.TestsInfrasructure;
using FluentAssertions;
using MediatR;
using Xunit;

namespace CQRS.Tests.Commands;

public class Commands
{
    public interface ICommand: IRequest
    { }

    public interface ICommandHandler<in T>: IRequestHandler<T> where T : ICommand
    { }

    public interface ICommandBus
    {
        Task Send(ICommand command);
    }

    public class CommandBus: ICommandBus
    {
        private readonly IMediator mediator;

        public CommandBus(IMediator mediator)
        {
            this.mediator = mediator;
        }

        public Task Send(ICommand command)
        {
            return mediator.Send(command);
        }
    }

    public class CreateIssueCommand: ICommand
    {
        public string Name { get; }

        public CreateIssueCommand(string name)
        {
            Name = name;
        }
    }

    public interface IIssueApplicationService
    {
        Task CreateIssue(CreateIssueCommand command);
    }

    public class IssueApplicationService: IIssueApplicationService
    {
        private readonly ICommandBus commandBus;

        public IssueApplicationService(ICommandBus commandBus)
        {
            this.commandBus = commandBus;
        }

        public Task CreateIssue(CreateIssueCommand command)
        {
            return commandBus.Send(command);
        }
    }

    public interface IAppWrtiteModel
    {
        IList<string> Issues { get; }
    }

    public class AppWriteModel: IAppWrtiteModel
    {
        public IList<string> Issues { get; }

        public AppWriteModel(params string[] issues)
        {
            Issues = new List<string>();
        }
    }

    public class CreateIssueCommandHandler: ICommandHandler<CreateIssueCommand>
    {
        private readonly IAppWrtiteModel writeModel;

        public CreateIssueCommandHandler(IAppWrtiteModel writeModel)
        {
            this.writeModel = writeModel;
        }

        public Task<Unit> Handle(CreateIssueCommand message, CancellationToken cancellationToken = default)
        {
            writeModel.Issues.Add(message.Name);
            return Unit.Task;
        }
    }

    [Fact]
    public void GivenCommandWithData_WhenCommandIsSendToApplicationService_ThenWriteModelIsChanged()
    {
        var serviceLocator = new ServiceLocator();

        var writeModel = new AppWriteModel();
        var commandHandler = new CreateIssueCommandHandler(writeModel);
        serviceLocator.RegisterCommandHandler<CreateIssueCommand, CreateIssueCommandHandler>(commandHandler);

        var applicationService = new IssueApplicationService(new CommandBus(serviceLocator.GetMediator()));

        //Given
        var createdIssueName = "cleaning";

        var command = new CreateIssueCommand(createdIssueName);

        //When
        applicationService.CreateIssue(command);

        //Then
        writeModel.Issues.Should().HaveCount(1);
        writeModel.Issues.Should().BeEquivalentTo(createdIssueName);
    }
}
