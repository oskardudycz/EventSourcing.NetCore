using Microsoft.Extensions.DependencyInjection;

namespace Shipments.Packages;

internal static class Config
{
    internal static IServiceCollection AddPackages(this IServiceCollection services) =>
        services.AddScoped<IPackageService, PackageService>();
}
