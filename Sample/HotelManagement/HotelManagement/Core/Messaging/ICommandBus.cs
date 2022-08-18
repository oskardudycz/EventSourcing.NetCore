namespace HotelManagement.Core.Messaging;

public interface ICommandBus
{
    Task Send<TCommand>(TCommand command, CancellationToken ct);
}
