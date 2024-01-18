using Microsoft.Extensions.Configuration;

namespace Core.Configuration;

public static class ConfigurationExtensions
{
    public static T GetRequiredConfig<T>(this IConfiguration configuration, string configurationKey) =>
        configuration.GetRequiredSection(configurationKey).Get<T>()
               ?? throw new InvalidOperationException(
                   $"{typeof(T).Name} configuration wasn't found for '${configurationKey}' key");

    public static string GetRequiredConnectionString(this IConfiguration configuration, string configurationKey) =>
        configuration.GetConnectionString("Incidents")
        ?? throw new InvalidOperationException(
            $"Configuration string with name '${configurationKey}' was not found");
}
