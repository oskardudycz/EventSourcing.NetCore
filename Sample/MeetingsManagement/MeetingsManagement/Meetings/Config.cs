using Core.Commands;
using Core.Marten.Repository;
using Core.Queries;
using Core.Repositories;
using Marten;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Meetings
{
    public static class Config
    {
        public static IServiceCollection AddMeeting(this IServiceCollection services)
        {
            return services
                .AddScoped<IRepository<Meeting>, MartenRepository<Meeting>>()
                .AddCommandHandler<CreateMeeting, HandleCreateMeeting>()
                .AddCommandHandler<ScheduleMeeting, HandleScheduleMeeting>()
                .AddQueryHandler<GetMeeting, MeetingView?, HandleGetMeeting>();
        }

        public static void ConfigureMarten(StoreOptions options)
        {
            options.Projections.SelfAggregate<Meeting>();
            options.Projections.Add(new MeetingViewProjection());
        }
    }
}
