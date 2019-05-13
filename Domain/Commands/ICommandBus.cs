using System.Threading.Tasks;

namespace Domain.Commands
{
    public interface ICommandBus
    {
        Task Send<TCommand>(TCommand command) where TCommand : ICommand;
    }
}
