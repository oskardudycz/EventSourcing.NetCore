using System.Text.Json;

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

        return client.PostAsync(path, new StringContent(JsonSerializer.Serialize(command)), cancellationToken);
    }

    public Task Put<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        var client = new HttpClient { BaseAddress = new Uri(url) };

        return client.PutAsync(path, new StringContent(JsonSerializer.Serialize(command)), cancellationToken);
    }

    public Task Delete<T>(string url, string path, T command, CancellationToken cancellationToken = default) where T: notnull
    {
        var client = new HttpClient { BaseAddress = new Uri(url) };

        return client.DeleteAsync(path, cancellationToken);
    }
}
