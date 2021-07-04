using MediatR;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Notifications.NotifyingByEvent;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Notifications
{
    public static class Config
    {
        public static void AddNotifications(this IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<MeetingCreated>, EmailNotifier>();
        }
    }
}
