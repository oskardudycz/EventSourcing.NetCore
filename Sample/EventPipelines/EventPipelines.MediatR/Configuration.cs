using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EventPipelines.MediatR
{
    public static class Configuration
    {
        public static IServiceCollection RouteEventsFromMediatR(this IServiceCollection services) =>
            services.AddSingleton(typeof(INotificationHandler<>), typeof(MediatorEventRouter<>));
    }
}
