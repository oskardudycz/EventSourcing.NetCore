using Microsoft.Extensions.DependencyInjection;

namespace Shipments.Packages
{
    internal static class Config
    {
        internal static IServiceCollection AddPackages(this IServiceCollection services)
        {
            return services.AddScoped<IPackageService, PackageService>();
        }
    }
}
