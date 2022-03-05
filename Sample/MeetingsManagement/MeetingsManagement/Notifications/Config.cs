using Core.Events;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Notifications.NotifyingByEvent;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Notifications;

public static class Config
{
    public static IServiceCollection AddNotifications(this IServiceCollection services) =>
        services.AddEventHandler<MeetingCreated, EmailNotifier>();
}
