using Core.Commands;
using Core.Requests;

namespace Core.Testing;

public class DummyExternalCommandBus : IExternalCommandBus
{
    public IList<ICommand> SentCommands { get; } = new List<ICommand>();

    public Task Post<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }

    public Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }

    public Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
    {
        SentCommands.Add(command);
        return Task.CompletedTask;
    }
}