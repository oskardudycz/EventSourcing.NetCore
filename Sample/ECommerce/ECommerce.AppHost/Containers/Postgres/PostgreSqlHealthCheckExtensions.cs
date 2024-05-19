// Taken from: https://github.com/davidfowl/WaitForDependenciesAspire/blob/main/WaitForDependencies.Aspire.Hosting/PostgreSqlHealthCheckExtensions.cs
using HealthChecks.NpgSql;

namespace Aspire.Hosting;

public static class PostgreSqlHealthCheckExtensions
{
    /// <summary>
    /// Adds a health check to the PostgreSQL server resource.
    /// </summary>
    public static IResourceBuilder<PostgresServerResource> WithHealthCheck(this IResourceBuilder<PostgresServerResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(cs))));

    /// <summary>
    /// Adds a health check to the PostgreSQL database resource.
    /// </summary>
    public static IResourceBuilder<PostgresDatabaseResource> WithHealthCheck(this IResourceBuilder<PostgresDatabaseResource> builder) =>
        builder.WithAnnotation(HealthCheckAnnotation.Create(cs => new NpgSqlHealthCheck(new NpgSqlHealthCheckOptions(cs))));
}
