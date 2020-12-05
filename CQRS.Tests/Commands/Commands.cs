using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CQRS.Tests.TestsInfrasructure;
using MediatR;
using SharpTestsEx;
using Xunit;

namespace CQRS.Tests.Commands
{
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
            private readonly IMediator _mediator;

            public CommandBus(IMediator mediator)
            {
                _mediator = mediator;
            }

            public Task Send(ICommand command)
            {
                return _mediator.Send(command);
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
            private readonly ICommandBus _commandBus;

            public IssueApplicationService(ICommandBus commandBus)
            {
                _commandBus = commandBus;
            }

            public Task CreateIssue(CreateIssueCommand command)
            {
                return _commandBus.Send(command);
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
            private readonly IAppWrtiteModel _writeModel;

            public CreateIssueCommandHandler(IAppWrtiteModel writeModel)
            {
                _writeModel = writeModel;
            }

            public Task<Unit> Handle(CreateIssueCommand message, CancellationToken cancellationToken = default)
            {
                _writeModel.Issues.Add(message.Name);
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
            writeModel.Issues.Should().Have.Count.EqualTo(1);
            writeModel.Issues.Should().Have.SameValuesAs(createdIssueName);
        }
    }
}
