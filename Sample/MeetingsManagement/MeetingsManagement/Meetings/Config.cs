using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using JasperFx.Events;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Meetings;

public static class Config
{
    public static IServiceCollection AddMeeting(this IServiceCollection services) =>
        services
            .AddMartenRepository<Meeting>()
            .AddCommandHandler<CreateMeeting, HandleCreateMeeting>()
            .AddCommandHandler<ScheduleMeeting, HandleScheduleMeeting>()
            .AddQueryHandler<GetMeeting, MeetingView?, HandleGetMeeting>();

    public static void ConfigureMarten(StoreOptions options)
    {
        options.Projections.Snapshot<Meeting>(SnapshotLifecycle.Inline);
        options.Projections.Add(new MeetingViewProjection(), ProjectionLifecycle.Inline);
        options.Events.StreamIdentity = StreamIdentity.AsGuid;
    }
}
