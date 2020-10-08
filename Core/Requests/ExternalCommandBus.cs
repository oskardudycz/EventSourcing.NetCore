using System.Threading;
using System.Threading.Tasks;
using Core.Commands;
using RestSharp;

namespace Core.Requests
{
    public interface IExternalCommandBus
    {
        Task Post<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand;
        Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand;
        Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand;
    }

    public class ExternalCommandBus: IExternalCommandBus
    {
        public Task Post<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
        {
            var client = new RestClient(url);

            var request = new RestRequest(path, DataFormat.Json);
            request.AddJsonBody(command);

            return client.PostAsync<dynamic>(request, cancellationToken);
        }

        public Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
        {
            var client = new RestClient(url);

            var request = new RestRequest(path, DataFormat.Json);
            request.AddJsonBody(command);

            return client.PutAsync<dynamic>(request, cancellationToken);
        }

        public Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: ICommand
        {
            var client = new RestClient(url);

            var request = new RestRequest(path, DataFormat.Json);
            request.AddJsonBody(command);

            return client.DeleteAsync<dynamic>(request, cancellationToken);
        }
    }
}
