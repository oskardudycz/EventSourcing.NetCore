using Core.Streaming.Consumers;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Streaming
{
    public static class Config
    {
        public static IServiceCollection AddExternalEventConsumerBackgroundWorker(this IServiceCollection services)
        {
            return services.AddHostedService<ExternalEventConsumerBackgroundWorker>();
        }
    }
}
