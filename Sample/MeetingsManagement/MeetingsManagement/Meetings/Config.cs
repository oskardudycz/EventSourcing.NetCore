using Core.Marten.Repository;
using Core.Repositories;
using Marten;
using MediatR;
using MeetingsManagement.Meetings.CreatingMeeting;
using MeetingsManagement.Meetings.GettingMeeting;
using MeetingsManagement.Meetings.SchedulingMeeting;
using Microsoft.Extensions.DependencyInjection;

namespace MeetingsManagement.Meetings
{
    public static class Config
    {
        public static void AddMeeting(this IServiceCollection services)
        {
            services.AddScoped<IRepository<Meeting>, MartenRepository<Meeting>>();

            services.AddScoped<IRequestHandler<CreateMeeting, Unit>, HandleCreateMeeting>();

            services.AddScoped<IRequestHandler<ScheduleMeeting, Unit>, HandleScheduleMeeting>();

            services.AddScoped<IRequestHandler<GetMeeting, MeetingView?>, HandleGetMeeting>();
        }

        public static void ConfigureMarten(StoreOptions options)
        {
            options.Projections.SelfAggregate<Meeting>();
            options.Projections.Add(new MeetingViewProjection());
        }
    }
}
