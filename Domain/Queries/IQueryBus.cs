using System.Threading.Tasks;

namespace Domain.Queries
{
    public interface IQueryBus
    {
        Task<TResponse> Send<TResponse>(IQuery<TResponse> command);
    }
}
