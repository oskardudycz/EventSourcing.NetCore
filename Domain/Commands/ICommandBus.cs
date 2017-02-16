using System.Threading.Tasks;

namespace Domain.Commands
{
    public interface ICommandBus
    {
        Task Send(ICommand command);
    }
}
