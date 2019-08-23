using MeetingsManagement.Meetings;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Configuration
{
    public static class ModuleConfig
    {
        public static void AddMeetingsManagement(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config);
            services.AddMeeting();
        }
    }
}
