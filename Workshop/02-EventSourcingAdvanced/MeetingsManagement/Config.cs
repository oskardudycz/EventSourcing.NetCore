using MeetingsManagement.Meetings;
using MeetingsManagement.Notifications;
using MeetingsManagement.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement
{
    public static class Config
    {
        public static void AddMeetingsManagement(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config, options =>
            {
                Meetings.Config.ConfigureMarten(options);
            });
            services.AddMeeting();
            services.AddNotifications();
        }
    }
}
