using System.Text.Json;
using RestSharp;

namespace Core.Requests;

public interface IExternalCommandBus
{
    Task Post<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull;
    Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull;
    Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull;
}

public class ExternalCommandBus: IExternalCommandBus
{
    public Task Post<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        var client = new HttpClient { BaseAddress = new Uri(url) };
        // //var client = new RestClient(url);
        //
        // var request = new RestRequest(path, Method.Post);
        // request.AddBody(command, ContentType.Json);

        return client.PostAsync(path, new StringContent(JsonSerializer.Serialize(command)), cancellationToken);
    }

    public Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        var client = new RestClient(url);

        var request = new RestRequest(path, Method.Put);
        request.AddBody(command, ContentType.Json);

        return client.PutAsync<dynamic>(request, cancellationToken);
    }

    public Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        var client = new RestClient(url);

        var request = new RestRequest(path, Method.Delete);
        request.AddBody(command, ContentType.Json);

        return client.DeleteAsync<dynamic>(request, cancellationToken);
    }
}
