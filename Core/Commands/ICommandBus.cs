namespace Core.Commands;

public interface ICommandBus
{
    /// <summary>
    /// Sends commands finding and calling registered in DI container command handler
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <typeparam name="TCommand"></typeparam>
    /// <exception cref="InvalidOperationException">Throws when command handler not found</exception>
    /// <returns></returns>
    Task Send<TCommand>(TCommand command, CancellationToken ct = default) where TCommand : notnull;

    /// <summary>
    /// Tries to send commands finding and calling registered in DI container command handler.
    /// </summary>
    /// <param name="command"></param>
    /// <param name="ct"></param>
    /// <typeparam name="TCommand"></typeparam>
    /// <returns>true if command handler was found and handled, false otherwise</returns>
    Task<bool> TrySend<TCommand>(TCommand command, CancellationToken ct = default) where TCommand: notnull;
}
