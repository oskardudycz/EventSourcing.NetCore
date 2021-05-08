using Core.Marten;
using MeetingsManagement.Meetings;
using MeetingsManagement.Notifications;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement
{
    public static class Config
    {
        public static void AddMeetingsManagement(this IServiceCollection services, IConfiguration config)
        {
            services.AddMarten(config, Meetings.Config.ConfigureMarten);
            services.AddMeeting();
            services.AddNotifications();
        }
    }
}
