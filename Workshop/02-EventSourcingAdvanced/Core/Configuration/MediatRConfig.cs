using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Configuration
{
    public static class MediatRConfig
    {
        public static void AddMediatR(this IServiceCollection services)
        {
            services.AddScoped<IMediator, Mediator>();
            services.AddTransient<ServiceFactory>(sp => sp.GetService);
        }
    }
}
